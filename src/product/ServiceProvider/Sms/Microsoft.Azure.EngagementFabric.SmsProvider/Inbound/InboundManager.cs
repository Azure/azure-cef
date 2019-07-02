// <copyright file="InboundManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Configuration;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Report;
using Microsoft.Azure.EngagementFabric.SmsProvider.Store;
using Microsoft.Azure.EngagementFabric.SmsProvider.Utils;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Inbound
{
    public class InboundManager : IInboundManager
    {
        private static readonly TimeSpan MessageKeepPeriod = TimeSpan.FromHours(72);
        private static readonly TimeSpan MessageCleanupPeriod = TimeSpan.FromHours(1);
        private static readonly int ReadCountInBatch = 500;

        private ISmsStore store;
        private IInboundTelemetryManager telemetryManager;
        private IReportManager reportManager;
        private ICredentialManager credentialManager;
        private IDisposable subscriber;

        public InboundManager(
            ISmsStoreFactory factory,
            ServiceConfiguration configuration,
            IReportManager reportManager,
            ICredentialManager credentialManager)
        {
            this.store = factory.GetStore();
            this.telemetryManager = new InboundTelemetryManager(configuration);
            this.reportManager = reportManager;
            this.credentialManager = credentialManager;

            this.subscriber = Observable
                .Timer(TimeSpan.Zero, MessageCleanupPeriod)
                .Select(x => Observable.FromAsync(async () => await CleanupAsync()))
                .Concat()
                .Subscribe();
        }

        public void Dispose()
        {
            this.subscriber.Dispose();
        }

        public async Task<HttpResponseMessage> OnInboundMessageReceived(string connectorName, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var metadata = await this.credentialManager.GetMetadata(connectorName);
                if (metadata != null)
                {
                    InboundAgent agent = null;
                    try
                    {
                        agent = new InboundAgent(metadata.ConnectorUri);

                        // Get credential
                        var connectorId = await agent.ParseConnectorIdFromInboundMessageAsync(request, cancellationToken);

                        if (string.IsNullOrEmpty(connectorId))
                        {
                            SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(OnInboundMessageReceived), OperationStates.FailedMatch, $"Failed to get connector key for '{connectorName}'.");
                            return request.CreateResponse(HttpStatusCode.Unauthorized);
                        }

                        var identifier = new ConnectorIdentifier(connectorName, connectorId);
                        SmsConnectorCredential credential = null;

                        try
                        {
                            credential = await this.credentialManager.GetConnectorCredentialByIdAsync(identifier);
                        }
                        catch
                        {
                            SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(OnInboundMessageReceived), OperationStates.FailedMatch, $"Failed to get credential for {identifier}.");
                            return request.CreateResponse(HttpStatusCode.Unauthorized);
                        }

                        // Parse and save messages
                        var result = await agent.ParseInboundRequestAsync(request, credential.ToDataContract(metadata), cancellationToken);
                        SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(OnInboundMessageReceived), OperationStates.InProgress, $"Complete parsing code={result.Response.HttpStatusCode} message={result.Messages?.Count ?? 0}");

                        if (result.Type == InboundType.MoMessage)
                        {
                            await SaveMoMessageAsync(result.Messages, agent, cancellationToken);
                        }
                        else if (result.Type == InboundType.Report)
                        {
                            var saveResult = await SaveReportMessageAsync(result.Messages, identifier);
                            if (!saveResult)
                            {
                                result.Response.HttpStatusCode = (int)HttpStatusCode.InternalServerError;
                            }
                        }

                        SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(OnInboundMessageReceived), OperationStates.Succeeded, $"Complete parsing code={result.Response.HttpStatusCode} message={result.Messages?.Count ?? 0}");
                        return result.Response.ToHttpResponseMessage();
                    }
                    finally
                    {
                        if (agent != null)
                        {
                            agent.Dispose();
                        }
                    }
                }

                SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(OnInboundMessageReceived), OperationStates.FailedMatch, $"Inbound message received from invalid connector '{connectorName}'.");
                return request.CreateResponse(HttpStatusCode.Forbidden);
            }
            catch (Exception ex)
            {
                SmsProviderEventSource.Current.ErrorException(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnInboundMessageReceived), OperationStates.Failed, "Failed to process inbound message", ex);
                return request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        public async Task<List<InboundMessageDetail>> GetInboundMessages(string engagementAccount)
        {
            var messages = await this.telemetryManager.GetMessagesAsync(engagementAccount, DateTime.UtcNow.Subtract(MessageKeepPeriod), ReadCountInBatch);
            if (messages == null)
            {
                return new List<InboundMessageDetail>(0);
            }

            TaskHelper.FireAndForget(() => this.telemetryManager.DeleteMessagesAsync(messages));

            return messages.Select(r => new InboundMessageDetail
            {
                PhoneNumber = r.PhoneNumber,
                ExtendedCode = r.ExtendedCode,
                Signature = r.Signature,
                Message = r.Message,
                InboundTime = r.InboundTime
            }).ToList();
        }

        public Task OnAccountDeletedAsync(string engagementAccount)
        {
            // Do not wait for the completion of store clean up
            TaskHelper.FireAndForget(() => this.telemetryManager.DeleteMessagesAsync(engagementAccount));
            return Task.CompletedTask;
        }

        private async Task SaveMoMessageAsync(Dictionary<ConnectorIdentifier, List<InboundMessage>> messages, InboundAgent agent, CancellationToken cancellationToken)
        {
            if (messages == null || messages.Count <= 0)
            {
                return;
            }

            foreach (var kv in messages)
            {
                var sharedAccounts = await this.credentialManager.ListCredentialAssignmentsById(kv.Key, true);
                if (sharedAccounts == null || sharedAccounts.Count <= 0)
                {
                    continue;
                }

                var groups = kv.Value.Where(v => v.MoMessage != null).GroupBy(m => m.MoMessage.ExtendedCode);
                foreach (var group in groups)
                {
                    var extended = group.Key;
                    var segments = await agent.ParseExtendedCodeAsync(extended, Constants.ExtendedCodeSegmentLengths, cancellationToken);

                    if (segments != null && segments.Count != 3)
                    {
                        SmsProviderEventSource.Current.Error(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.SaveMoMessageAsync), OperationStates.Dropped, $"Dropped the message as connector parsing extended code error with segment={segments.Count}.");
                        continue;
                    }

                    var accountCode = segments?[0];
                    var sigCode = segments?[1];
                    var customCode = segments?[2];

                    ConnectorCredentialAssignment account = null;

                    // case #1: credential is not shared by multiple accounts
                    if ((segments == null || string.IsNullOrEmpty(accountCode)) && sharedAccounts.Count == 1)
                    {
                        account = sharedAccounts.SingleOrDefault();
                    }

                    // case #2: credentials are shared between accounts
                    else if (!string.IsNullOrEmpty(accountCode) && !string.IsNullOrEmpty(sigCode))
                    {
                        account = sharedAccounts.SingleOrDefault(a => a.ExtendedCode.Equals(accountCode, StringComparison.OrdinalIgnoreCase));
                    }

                    if (account != null)
                    {
                        Signature signature = null;
                        if (!string.IsNullOrEmpty(sigCode))
                        {
                            var signatureList = await this.store.ListSignaturesAsync(account.EngagementAccount, new Common.Pagination.DbContinuationToken(null), -1);
                            signature = signatureList?.Signatures?.SingleOrDefault(s => s.ExtendedCode.Equals(sigCode));
                        }
                        else
                        {
                            signature = new Signature
                            {
                                EngagementAccount = account.EngagementAccount,
                                Value = null
                            };
                        }

                        if (signature != null)
                        {
                            await this.telemetryManager.InsertInboundMessagesAsync(signature, group.ToList(), customCode);
                            continue;
                        }
                    }

                    SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.SaveMoMessageAsync), OperationStates.Dropped, $"Dropped the message as we cannot find the owner. connector={kv.Key} extended={extended}");
                }
            }
        }

        private async Task<bool> SaveReportMessageAsync(Dictionary<ConnectorIdentifier, List<InboundMessage>> messages, ConnectorIdentifier identifier)
        {
            if (messages == null || messages.Count <= 0)
            {
                return false;
            }

            var reports = new List<ReportDetail>();
            foreach (var kv in messages)
            {
                reports.AddRange(kv.Value.Where(v => v.ReportMessage != null).Select(v => v.ReportMessage).ToList());
            }

            if (reports.Count > 0)
            {
                var saved = await this.reportManager.OnReportPushedAsync(reports, identifier);
                return saved == reports.Count;
            }

            return false;
        }

        private async Task CleanupAsync()
        {
            await this.telemetryManager.CleanupMessageAsync(DateTime.UtcNow.Subtract(MessageKeepPeriod));
        }
    }
}
