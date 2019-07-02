// <copyright file="ReportManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Billing;
using Microsoft.Azure.EngagementFabric.EmailProvider.Configuration;
using Microsoft.Azure.EngagementFabric.EmailProvider.Credential;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.EmailProvider.Store;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    public class ReportManager : IReportManager
    {
        private string node;
        private IEmailStore store;
        private IReportTelemetryManager telemetryManager;
        private ICredentialManager credentialManager;
        private ServiceConfiguration configuration;

        public ReportManager(
            string node,
            IEmailStoreFactory factory,
            ServiceConfiguration configuration,
            BillingAgent billingAgent,
            MetricManager metricManager,
            ICredentialManager credentialManager)
        {
            this.node = node;
            this.store = factory.GetStore();
            this.telemetryManager = new ReportTelemetryManager(
                factory,
                configuration,
                billingAgent,
                metricManager);

            this.credentialManager = credentialManager;
            this.configuration = configuration;
        }

        public async Task OnMessageSentAsync(string engagementAccount, InputMessage message, EmailMessageInfoExtension extension)
        {
            await this.telemetryManager.OnMessageSentAsync(message, extension);
        }

        public async Task OnDispatchCompleteAsync(OutputResult outputResult)
        {
            await this.telemetryManager.OnMessageDispatchedAsync(outputResult);
        }

        public async Task PullReportsAsync()
        {
            string account = null;

            // Loop until no more account needs to process
            do
            {
                // Get accounts acquired by this node
                account = await this.telemetryManager.AcquireNextAccountForPullingReportAsync(this.node);
                if (!string.IsNullOrEmpty(account))
                {
                    EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportsAsync), OperationStates.Starting, $"Processor {this.node} acquired lease for account {account}");

                    // Get in-progress messages
                    var messageIds = await this.telemetryManager.ListInProgressMessagesAsync(account);
                    EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportsAsync), OperationStates.Empty, $"Processor {this.node} pulled {messageIds?.Count ?? 0} in-progress messages for account {account}");

                    if (messageIds.Count > 0)
                    {
                        // Get reports
                        var agent = await this.GetReportAgent(account);
                        var reports = await agent.GetReportsAsync(messageIds, CancellationToken.None);

                        EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportsAsync), OperationStates.Empty, $"Processor {this.node} pulled {reports?.Reports?.Count ?? 0} reports for account {account}");

                        if (reports?.Reports.Count > 0)
                        {
                            // Fire event to update telemetry
                            foreach (var report in reports.Reports)
                            {
                                await this.telemetryManager.OnReportUpdatedAsync(account, report);
                            }

                            EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportsAsync), OperationStates.InProgress, $"Processor {this.node} complete notification to telemetry manager for account {account}");
                        }
                    }

                    await this.telemetryManager.ReleaseAccountForPullingReportAsync(this.node, account);
                    EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportsAsync), OperationStates.Succeeded, $"Processor {this.node} released lease for account {account}");
                }
            }
            while (!string.IsNullOrEmpty(account));

            EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportsAsync), OperationStates.Succeeded, $"Processor {this.node} completed pulling");
        }

        public async Task<MessageRecord> GetReportAsync(string engagementAccount, string messageId)
        {
            // Get from table
            var record = await this.telemetryManager.GetMessageReportAsync(engagementAccount, messageId);
            if (record == null)
            {
                return null;
            }

            // Get from connector so that the report will be always the latest
            var agent = await this.GetReportAgent(engagementAccount);
            var reportList = await agent.GetReportsAsync(new List<MessageIdentifer> { new MessageIdentifer(record.MessageId, record.CustomMessageId) }, CancellationToken.None);
            if (reportList?.Reports?.Count == 1)
            {
                var latest = reportList.Reports.First();
                if (record.Targets != latest.TotalTarget ||
                    record.Delivered != latest.TotalDelivered ||
                    record.Opened != latest.TotalOpened ||
                    record.Clicked != latest.TotalClicked)
                {
                    record.Targets = latest.TotalTarget;
                    record.Delivered = latest.TotalDelivered;
                    record.Opened = latest.TotalOpened;
                    record.Clicked = latest.TotalClicked;

                    await this.telemetryManager.OnReportUpdatedAsync(engagementAccount, latest);
                }
            }

            return record;
        }

        public async Task OnAccountCreatedOrUpdatedAsync(string engagementAccount)
        {
            await this.telemetryManager.OnAccountCreatedOrUpdatedAsync(engagementAccount);
        }

        public async Task OnAccountDeletedAsync(string engagementAccount)
        {
            await this.telemetryManager.OnAccountDeletedAsync(engagementAccount);
        }

        private async Task<ReportAgent> GetReportAgent(string engagementAccount)
        {
            // Get Credential
            var credential = await this.credentialManager.GetConnectorCredentialContractAsync(engagementAccount);

            // Get Email Account
            var emailAccount = await this.credentialManager.GetEmailAccountAsync(engagementAccount);

            // Get agent
            return new ReportAgent(credential, emailAccount, this.configuration);
        }
    }
}
