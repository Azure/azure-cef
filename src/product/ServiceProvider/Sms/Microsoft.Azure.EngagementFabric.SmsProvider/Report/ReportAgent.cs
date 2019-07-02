// <copyright file="ReportAgent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    public class ReportAgent
    {
        private static readonly string AgentIdFormat = "ReportAgent_{0}_{1}";
        private static readonly TimeSpan PullingInterval = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan ActiveInternal = TimeSpan.FromHours(24);
        private static readonly int MaxRetry = 3;

        private IDisposable subscriber;
        private object thisLock;
        private ConnectorCredential credential;
        private ISmsConnector connector;
        private IReportManager reportManager;
        private DateTime? lastMessageSentTime;
        private CancellationTokenSource ts;
        private int retry;

        public ReportAgent(ConnectorCredential credential, IReportManager reportManager)
        {
            this.credential = credential;
            this.reportManager = reportManager;

            this.thisLock = new object();
        }

        public ConnectorCredential Credential => this.credential;

        public void OnStart(DateTime startTime, string requestId)
        {
            lock (this.thisLock)
            {
                this.lastMessageSentTime = startTime;

                if (this.ts == null)
                {
                    this.ts = new CancellationTokenSource();
                }

                if (this.connector == null)
                {
                    var serviceUri = new Uri(this.credential.ConnectorUri);
                    var actorId = new ActorId(string.Format(AgentIdFormat, this.credential.ConnectorName, this.credential.ConnectorId));
                    this.connector = ActorProxy.Create<ISmsConnector>(actorId, serviceUri);
                }

                if (this.subscriber == null)
                {
                    this.subscriber = Observable
                        .Timer(TimeSpan.Zero, PullingInterval)
                        .Select(x => Observable.FromAsync(async () => await PullAsync()))
                        .Concat()
                        .Subscribe(reports => ProcessMessageReports(reports));

                    SmsProviderEventSource.Current.Info(requestId ?? SmsProviderEventSource.EmptyTrackingId, this, nameof(OnStart), OperationStates.Succeeded, $"Report agent started. connectorName={this.credential.ConnectorName} connectorKey={this.credential.ConnectorId}");
                }
            }
        }

        public void OnMessageSent(InputMessage message, string requestId)
        {
            if (message.Targets == null || message.Targets.Count <= 0)
            {
                return;
            }

            this.OnStart(message.MessageInfo.SendTime, requestId);
        }

        public void UnSubscribe()
        {
            lock (this.thisLock)
            {
                if (this.subscriber != null)
                {
                    this.subscriber.Dispose();
                    this.subscriber = null;

                    SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(UnSubscribe), OperationStates.Succeeded, $"Report agent stopped. connectorName={this.credential.ConnectorName} connectorKey={this.credential.ConnectorId}");
                }

                if (this.ts != null)
                {
                    this.ts.Cancel();
                    this.ts.Dispose();
                    this.ts = null;
                }

                if (this.connector != null)
                {
                    this.connector = null;
                }

                this.reportManager.OnAgentUnSubscribed(this);
            }
        }

        private async Task<List<ReportDetail>> PullAsync()
        {
            try
            {
                if (this.connector == null)
                {
                    SmsProviderEventSource.Current.Error(SmsProviderEventSource.EmptyTrackingId, this, nameof(PullAsync), OperationStates.Failed, $"Report agent cannot get client for connector={this.credential.ConnectorName} key={this.credential.ConnectorId}. Agent stopped.");
                    UnSubscribe();
                    return null;
                }

                var response = await this.connector.FetchMessageReportsAsync(this.credential, this.ts.Token);
                if (response.RequestOutcome != RequestOutcome.SUCCESS)
                {
                    this.retry++;
                    if (this.retry > MaxRetry)
                    {
                        SmsProviderEventSource.Current.Error(SmsProviderEventSource.EmptyTrackingId, this, nameof(PullAsync), OperationStates.Failed, $"Report agent faild for {MaxRetry} times for connector={this.credential.ConnectorName} key={this.credential.ConnectorId}. Agent stopped.");
                        this.retry = 0;
                        UnSubscribe();
                    }
                    else
                    {
                        SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(PullAsync), OperationStates.FailedNotFaulting, $"Report agent faild for {MaxRetry} times for connector={this.credential.ConnectorName} key={this.credential.ConnectorId}, outcome={response.RequestOutcome}.");
                    }
                }
                else
                {
                    this.retry = 0;
                }

                return response.Details;
            }
            catch (Exception ex)
            {
                SmsProviderEventSource.Current.ErrorException(SmsProviderEventSource.EmptyTrackingId, this, nameof(PullAsync), OperationStates.Failed, "Failed to pull report", ex);
                return null;
            }
        }

        private void ProcessMessageReports(List<ReportDetail> reports)
        {
            if (reports == null || reports.Count <= 0)
            {
                return;
            }

            TaskHelper.FireAndForget(
                () => this.reportManager.OnReportPulledAsync(this, reports),
                ex => SmsProviderEventSource.Current.ErrorException(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.ProcessMessageReports), OperationStates.Failed, "ReportManager.OnReportPulled failed with exception", ex));

            if (this.lastMessageSentTime != null && this.lastMessageSentTime.Value.Add(ActiveInternal) <= DateTime.UtcNow)
            {
                SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(ProcessMessageReports), OperationStates.Succeeded, $"Report agent stopping (idle). connectorName={this.credential.ConnectorName} connectorKey={this.credential.ConnectorId} lastMessageSentTime={this.lastMessageSentTime}");
                UnSubscribe();
            }
        }
    }
}
