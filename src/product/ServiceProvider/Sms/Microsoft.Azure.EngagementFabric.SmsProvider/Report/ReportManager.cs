// <copyright file="ReportManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Billing;
using Microsoft.Azure.EngagementFabric.SmsProvider.Configuration;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Microsoft.Azure.EngagementFabric.SmsProvider.Mdm;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Store;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    public class ReportManager : IReportManager
    {
        private static readonly Uri MetaServiceUri = new Uri("fabric:/SmsApp/SmsMetaService");
        private static readonly TimeSpan PendingReceiveTimeout = TimeSpan.FromHours(24);

        private readonly ISmsStore store;
        private readonly IReportTelemetryManager telemetryManager;
        private readonly ICredentialManager credentialManager;
       //  private readonly ITimeSeriesManager timeSeriesManager;
        private readonly ConcurrentDictionary<string, ReportAgent> agents;

        public ReportManager(
            ISmsStoreFactory factory,
            ServiceConfiguration configuration,
            BillingAgent billingAgent,
            MetricManager metricManager,
            ICredentialManager credentialManager 
            /*,ITimeSeriesManager timeSeriesManager*/)
        {
            this.store = factory.GetStore();
            this.telemetryManager = new ReportTelemetryManager(
                factory,
                configuration,
                billingAgent,
                metricManager,
                credentialManager);

            this.agents = new ConcurrentDictionary<string, ReportAgent>();
            this.credentialManager = credentialManager;
           // this.timeSeriesManager = timeSeriesManager;

            this.Init().Wait();
        }

        public void Dispose()
        {
            if (this.agents.Count > 0)
            {
                foreach (var agent in this.agents)
                {
                    agent.Value.UnSubscribe();
                }
            }
        }

        public async Task<MessageDetails> GetMessageAsync(string engagementAccount, string messageId, int count, TableContinuationToken continuationToken)
        {
            return await this.telemetryManager.GetMessageHistoryAsync(engagementAccount, messageId, count, continuationToken);
        }

        public async Task<PerMessageAggregationList> GetPerMessageAggregationAsync(
            string engagementAccount,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int count,
            TableContinuationToken continuationToken)
        {
            return await this.telemetryManager.GetPerMessageAggregationAsync(
                engagementAccount,
                startTime,
                endTime,
                count,
                continuationToken);
        }

        public Task<PerPeriodAggregationList> GetPerPeriodAggregationAsync(
            string requestId,
            string engagementAccount,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes)
        {

           // return null;
            //var tasks = Enum.GetNames(typeof(MessageCategory)).Select(category =>
            //{
            //    var dimensionCombination = new Dictionary<string, string>
            //    {
            //        {
            //            MetricManager.DimensionEngagementAccount,
            //            engagementAccount
            //        },
            //        {
            //            MetricManager.DimensionMessageCategory,
            //            category
            //        }
            //    };

            //    var getTimeSeriesTask = this.timeSeriesManager.GetTimeSeriesAsync(
            //        requestId,
            //        dimensionCombination,
            //        DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            //        startTime,
            //        endTime,
            //        seriesResolutionInMinutes);

            //    return getTimeSeriesTask.ContinueWith(t => new KeyValuePair<string, IReadOnlyDictionary<string, TimeSeries>>(category, t.Result));
            //});

            //var dictionariesByCategory = await Task.WhenAll(tasks);

            //// Merge multiple time series into single list
            //var result = new Dictionary<long, Dictionary<string, object>>();
            //foreach (var categoryPair in dictionariesByCategory)
            //{
            //    foreach (var pair in categoryPair.Value)
            //    {
            //        foreach (var datapoint in pair.Value.Datepoints)
            //        {
            //            if (!result.TryGetValue(datapoint.Key, out var values))
            //            {
            //                values = new Dictionary<string, object>();
            //                result.Add(datapoint.Key, values);
            //            }

            //            values.Add($"{pair.Key}.{categoryPair.Key}", datapoint.Value);
            //        }
            //    }
            //}

            //var summary = result.Values.SelectMany(p => p)
            //    .GroupBy(pair => pair.Key, pair => (double)pair.Value)
            //    .ToDictionary(group => group.Key, group => (object)group.Sum());

            //// Add timestamp
            //foreach (var pair in result)
            //{
            //    pair.Value.Add(PerPeriodAggregationList.TimestampKey, pair.Key);
            //}

            return Task.FromResult(new PerPeriodAggregationList(
                null,
                null,
                startTime,
                endTime,
                seriesResolutionInMinutes));
        }

        public async Task OnMessageSentAsync(string engagementAccount, InputMessage message, SmsMessageInfoExtension extension)
        {
            if (message.Targets == null || message.Targets.Count <= 0)
            {
                return;
            }

            // Init telemetry
            await this.telemetryManager.OnMessageSentAsync(message, extension);

            // Get connector metadata
            var metadata = await this.credentialManager.GetMetadata(message.ConnectorCredential.ConnectorName);
            if (metadata.ReportType != ConnectorMetadata.ConnectorInboundType.Pull)
            {
                return;
            }

            // Update agent metadata
            var meta = await this.store.GetAgentMetadataAsync(message.ConnectorCredential);
            if (meta == null)
            {
                meta = new AgentMetadata();
                meta.ConnectorName = message.ConnectorCredential.ConnectorName;
                meta.ConnectorId = message.ConnectorCredential.ConnectorId;
            }

            meta.LastMessageSendTime = message.MessageInfo.SendTime;
            meta.PendingReceive += message.Targets.Count;

            await this.store.CreateOrUpdateAgentMetadataAsync(meta);

            // Get agent
            var agent = this.GetAgent(message.ConnectorCredential);
            if (agent == null)
            {
                return;
            }

            // Notify agent of message sent
            agent.OnMessageSent(message, message.MessageInfo.TrackingId);
        }

        public async Task OnDispatchCompleteAsync(OutputResult outputResult)
        {
            // If dispatch failed, upate the telemetry
            if (!outputResult.Delivered && outputResult.Targets != null && outputResult.Targets.Count > 0)
            {
                // Update record detail
                var details = outputResult.Targets.Select(t => new ReportDetail
                {
                    PhoneNumber = t,
                    MessageId = outputResult.MessageId.ToString(),
                    State = ErrorCodeHelper.ConvertMessageStateFromRequestOutcome(outputResult.DeliveryResponse.DeliveryOutcome),
                    StateDetail = outputResult.DeliveryResponse.DeliveryDetail
                }).ToList();

                var updated = await this.UpdateReportAndDetails(details, outputResult.ConnectorIdentifier);

                // Update agent meta if agent exist
                var key = this.GetAgentKey(outputResult.ConnectorIdentifier);
                if (updated > 0 && !string.IsNullOrEmpty(key) && this.agents.TryGetValue(key, out ReportAgent agent))
                {
                    var meta = await this.store.GetAgentMetadataAsync(outputResult.ConnectorIdentifier);
                    if (meta != null)
                    {
                        meta.PendingReceive -= Math.Min(outputResult.Targets.Count, meta.PendingReceive);
                        await this.store.CreateOrUpdateAgentMetadataAsync(meta);

                        if (meta.PendingReceive <= 0)
                        {
                            SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnDispatchCompleteAsync), OperationStates.Succeeded, $"Report agent stopping (complete). connectorName={agent.Credential.ConnectorName} connectorKey={agent.Credential.ConnectorId}");
                            agent.UnSubscribe();
                        }
                    }
                }
            }

            // If succeed with custom message Id, update the summary
            else if (outputResult.Delivered && !string.IsNullOrEmpty(outputResult.DeliveryResponse.CustomMessageId))
            {
                await this.telemetryManager.OnMessageDispatchedAsync(
                    outputResult.EngagementAccount,
                    outputResult.MessageId.ToString(),
                    outputResult.DeliveryResponse.CustomMessageId,
                    outputResult.Targets.ToList(),
                    outputResult.ConnectorIdentifier);
            }

            await this.telemetryManager.InsertMessageBatchRecordAsync(outputResult);
        }

        public async Task OnReportPulledAsync(ReportAgent agent, List<ReportDetail> reports)
        {
            // Update report table
            var updated = await this.UpdateReportAndDetails(reports, agent.Credential);
            if (updated <= 0)
            {
                return;
            }

            // Update meta db
            var meta = await this.store.GetAgentMetadataAsync(agent.Credential);
            if (meta == null)
            {
                return;
            }

            meta.PendingReceive -= Math.Min(meta.PendingReceive, updated);
            meta.LastReportUpdateTime = DateTime.UtcNow;
            await this.store.CreateOrUpdateAgentMetadataAsync(meta);

            // Stop agent if no more detail to pull
            if (meta.PendingReceive <= 0)
            {
                SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnReportPulledAsync), OperationStates.Succeeded, $"Report agent stopping (complete). connectorName={agent.Credential.ConnectorName} connectorKey={agent.Credential.ConnectorId}");
                agent.UnSubscribe();
            }
        }

        public async Task<int> OnReportPushedAsync(List<ReportDetail> reports, ConnectorIdentifier identifier)
        {
            return await this.UpdateReportAndDetails(reports, identifier);
        }

        public void OnAgentUnSubscribed(ReportAgent agent)
        {
            var key = this.GetAgentKey(agent.Credential);
            this.agents.TryRemove(key, out ReportAgent deleted);
        }

        public async Task OnAccountCreatedOrUpdatedAsync(string engagementAccount)
        {
            await this.telemetryManager.CreateMessageHistoryIfNotExistAsync(engagementAccount);
        }

        public async Task OnAccountDeletedAsync(string engagementAccount)
        {
            await this.telemetryManager.DeleteMessageHistoryAsync(engagementAccount);
        }

        private async Task Init()
        {
            // Get metadata
            var metaList = await this.store.ListAgentMetadataAsync();
            if (metaList == null || metaList.Count <= 0)
            {
                return;
            }

            // Check if there's any metadata has pending receive message
            var active = metaList.Where(
                m => m.PendingReceive > 0 &&
                m.LastMessageSendTime != null && m.LastMessageSendTime.Value.Add(PendingReceiveTimeout) > DateTime.UtcNow).ToList();

            if (active.Count <= 0)
            {
                return;
            }

            // Start agent
            SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.Init), OperationStates.Starting, $"Report manager find {active.Count} agent to be started during init.");
            var tasks = new List<Task>();
            foreach (var meta in active)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var metadata = await this.credentialManager.GetMetadata(meta.ConnectorName);
                        if (metadata.ReportType == ConnectorMetadata.ConnectorInboundType.Pull)
                        {
                            var credential = await this.credentialManager.GetConnectorCredentialByIdAsync(meta);

                            SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.Init), OperationStates.InProgress, $"Report agent starting (init). connectorName={credential.ConnectorName} connectorKey={credential.ConnectorId}");
                            this.GetAgent(credential.ToDataContract(metadata))?.OnStart(DateTime.UtcNow, null);
                            SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.Init), OperationStates.Succeeded, $"Report agent started (init). connectorName={credential.ConnectorName} connectorKey={credential.ConnectorId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SmsProviderEventSource.Current.ErrorException(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.Init), OperationStates.Failed, $"Report agent failed to start", ex);
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }

        private ReportAgent GetAgent(ConnectorCredential credential)
        {
            var key = this.GetAgentKey(credential);
            if (!this.agents.ContainsKey(key))
            {
                this.agents.TryAdd(key, new ReportAgent(credential, this));
            }

            if (this.agents.TryGetValue(key, out ReportAgent agent))
            {
                return agent;
            }

            SmsProviderEventSource.Current.Error(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.GetAgent), OperationStates.Failed, $"Report manager cannot get agent for connectorName={credential.ConnectorName} connectorKey={credential.ConnectorId}");
            return null;
        }

        private string GetAgentKey(ConnectorIdentifier identifer)
        {
            return identifer != null ? string.Concat(identifer.ConnectorName, identifer.ConnectorId) : null;
        }

        private async Task<int> UpdateReportAndDetails(List<ReportDetail> reports, ConnectorIdentifier identifier)
        {
            if (reports == null || reports.Count <= 0)
            {
                return 0;
            }

            // Update telemetry
            var count = 0;
            var groups = reports.GroupBy(r => new { r.MessageId, r.CustomMessageId });
            foreach (var group in groups)
            {
                var succeed = group.Where(r => r.State == MessageState.DELIVERED).Count();
                var failed = group.Where(r => r.State != MessageState.DELIVERED).Count();
                var updated = await this.telemetryManager.OnMessageReportUpdatedAsync(
                    group.Key.MessageId,
                    group.Key.CustomMessageId,
                    group.ToList(),
                    identifier);

                if (updated)
                {
                    count += group.Count();
                }
            }

            return count;
        }
    }
}
