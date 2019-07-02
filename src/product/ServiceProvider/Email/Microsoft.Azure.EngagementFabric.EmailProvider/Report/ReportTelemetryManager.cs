// <copyright file="ReportTelemetryManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Billing;
using Microsoft.Azure.EngagementFabric.EmailProvider.Configuration;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.EmailProvider.Store;
using Microsoft.Azure.EngagementFabric.EmailProvider.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    using Report = Email.Common.Contract.Report;

    public class ReportTelemetryManager : IReportTelemetryManager
    {
        // Internal tables
        private static readonly string MessageAuditTableName = "emailaudit";
        private static readonly string ReportInprogressTableName = "emailreportinprogress";
        private static readonly string ReportLeaseTableName = "emailreportlease";

        // Customer-facing tables
        private static readonly string MessageHistoryTableName = "emailhistory{0}";

        private CloudTableClient client;
        private BillingAgent billingAgent;
        private MetricManager metricManager;

        private CloudTable auditTable;
        private CloudTable reportInProgressTable;
        private CloudTable reportLeaseTable;

        private IEmailStore store;

        public ReportTelemetryManager(
            IEmailStoreFactory factory,
            ServiceConfiguration configuration,
            BillingAgent billingAgent,
            MetricManager metricManager)
        {
            this.store = factory.GetStore();

            var account = CloudStorageAccount.Parse(configuration.TelemetryStoreConnectionString);
            this.client = account.CreateCloudTableClient();

            this.auditTable = this.client.GetTableReference(MessageAuditTableName);
            this.reportInProgressTable = this.client.GetTableReference(ReportInprogressTableName);
            this.reportLeaseTable = this.client.GetTableReference(ReportLeaseTableName);

            this.auditTable.CreateIfNotExists();
            this.reportInProgressTable.CreateIfNotExists();
            this.reportLeaseTable.CreateIfNotExists();

            this.billingAgent = billingAgent;
            this.metricManager = metricManager;
        }

        public async Task OnMessageSentAsync(InputMessage message, EmailMessageInfoExtension extension)
        {
            // Create record in history table
            var historyTable = await GetHistoryTableAsync(message.MessageInfo.EngagementAccount);
            if (historyTable == null)
            {
                // Create the table in case any issue in account initialize
                historyTable = await GetHistoryTableAsync(message.MessageInfo.EngagementAccount, true);
            }

            var history = new MessageHistoryTableEntity(message, extension);
            await MessageHistoryTableEntity.InsertOrMergeAsync(historyTable, history);

            // Create record in audit table
            var audit = new MessageAuditTableEntity(history);
            await MessageAuditTableEntity.InsertOrMergeAsync(auditTable, audit);
        }

        public async Task OnMessageDispatchedAsync(OutputResult outputResult)
        {
            // If dispatch failed, update the history table with target -1
            if (!outputResult.Delivered)
            {
                var historyTable = await GetHistoryTableAsync(outputResult.EngagementAccount);
                var historyEntity = new MessageHistoryTableEntity(outputResult.EngagementAccount, outputResult.MessageId.ToString());
                if (historyEntity == null)
                {
                    EmailProviderEventSource.Current.Warning(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.OnMessageDispatchedAsync), OperationStates.Dropped, $"Does not find record for messageId={outputResult.MessageId}");
                    return;
                }

                historyEntity.Targets = -1;
                await MessageHistoryTableEntity.InsertOrMergeAsync(historyTable, historyEntity);
            }

            // If succeed, update the in-progress table
            else
            {
                // Insert in-progress table
                var inProgressEntity = new ReportInProgressTableEntity(outputResult.EngagementAccount, outputResult.MessageId, outputResult.DeliveryResponse.CustomMessageId);
                await ReportInProgressTableEntity.InsertOrMergeAsync(reportInProgressTable, inProgressEntity);
            }
        }

        public async Task<bool> OnReportUpdatedAsync(string engagementAccount, Report report)
        {
            try
            {
                // Get record in in-progress table
                var inProgressEntity = await ReportInProgressTableEntity.GetAsync(reportInProgressTable, engagementAccount, report.MessageIdentifer.MessageId);
                if (inProgressEntity == null)
                {
                    // Record should be already in history table
                    var historyTable = await GetHistoryTableAsync(engagementAccount);
                    var historyEntity = await MessageHistoryTableEntity.GetAsync(historyTable, engagementAccount, report.MessageIdentifer.MessageId);
                    if (historyEntity == null)
                    {
                        EmailProviderEventSource.Current.Warning(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.OnReportUpdatedAsync), OperationStates.Dropped, $"Does not find record for messageId={report.MessageIdentifer.MessageId}");
                        return false;
                    }

                    // TODO: if after 24h we get new number of target shall we charge?
                    historyEntity.Targets = report.TotalTarget;
                    historyEntity.Delivered = report.TotalDelivered;
                    historyEntity.Opened = report.TotalOpened;
                    historyEntity.Clicked = report.TotalClicked;
                    historyEntity.LastUpdateTime = DateTime.UtcNow;

                    await MessageHistoryTableEntity.InsertOrMergeAsync(historyTable, historyEntity);
                    return true;
                }

                var lastTarget = inProgressEntity.Targets;

                // Update the record in in-progress table
                inProgressEntity.Targets = report.TotalTarget;
                inProgressEntity.Delivered = report.TotalDelivered;
                inProgressEntity.Opened = report.TotalOpened;
                inProgressEntity.Clicked = report.TotalClicked;
                inProgressEntity.LastUpdateTime = DateTime.UtcNow;

                var updated = await ReportInProgressTableEntity.TryUpdateAsync(reportInProgressTable, inProgressEntity);

                // New target sent, do the charge (only if update succeed)
                if (updated && lastTarget < report.TotalTarget)
                {
                    try
                    {
                        var delta = report.TotalTarget - lastTarget;

                        // Get account detail
                        var account = await this.store.GetAccountAsync(engagementAccount);
                        metricManager.LogDeliverSuccess(delta, engagementAccount, account?.SubscriptionId ?? string.Empty);

                        var usage = new ResourceUsageRecord();
                        usage.EngagementAccount = engagementAccount;
                        usage.UsageType = ResourceUsageType.EmailMessage;
                        usage.Quantity = delta;

                        await this.billingAgent.StoreBillingUsageAsync(new List<ResourceUsageRecord> { usage }, CancellationToken.None);

                        EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.OnReportUpdatedAsync), OperationStates.Succeeded, $"Usage pushed to billing service account={engagementAccount} quantity={usage.Quantity}");
                    }
                    catch (Exception ex)
                    {
                        // We should monitor for billing failure
                        EmailProviderEventSource.Current.CriticalException(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.OnReportUpdatedAsync), OperationStates.Failed, $"Failed at pushing to billing service account={engagementAccount}", ex);
                    }
                }

                // If the record in in-progress for 24h, treat it as completed and move it to history table
                if (inProgressEntity.Timestamp.AddHours(Constants.ReportInProgressIntervalByHours) < DateTime.UtcNow)
                {
                    // Record should be already in history table
                    var historyTable = await GetHistoryTableAsync(engagementAccount);
                    var historyEntity = new MessageHistoryTableEntity(inProgressEntity);

                    await MessageHistoryTableEntity.InsertOrMergeAsync(historyTable, historyEntity);

                    await ReportInProgressTableEntity.DeleteAsync(reportInProgressTable, inProgressEntity);
                }

                return true;
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.OnReportUpdatedAsync), OperationStates.Failed, string.Empty, ex);
                return false;
            }
        }

        public async Task<string> AcquireNextAccountForPullingReportAsync(string processor)
        {
            // Get all accounts with no processor
            var idleAccounts = await ReportLeaseTableEntity.ListAsync(reportLeaseTable, null);
            if (idleAccounts == null || idleAccounts.Count <= 0)
            {
                return null;
            }

            // Try to acquire one and only one account
            var now = DateTime.UtcNow;
            foreach (var account in idleAccounts)
            {
                // We will ensure that the account will be processed only once every ReportPullingIntervalByMinutes - 5 (55) minutes
                // If no other processor, the same processor must to process the account in next round
                if (account.LastProcessTime != null && account.LastProcessTime.Value.AddMinutes(Constants.ReportPullingIntervalByMinutes - 5) >= now)
                {
                    continue;
                }

                if (await ReportLeaseTableEntity.TryAcquireLeaseAsync(reportLeaseTable, account.EngagementAccount, processor))
                {
                    return account.EngagementAccount;
                }
            }

            return null;
        }

        public async Task ReleaseAccountForPullingReportAsync(string processor, string engagementAccount)
        {
            await ReportLeaseTableEntity.TryReleaseLeaseAsync(reportLeaseTable, engagementAccount, processor);
        }

        public async Task<List<MessageIdentifer>> ListInProgressMessagesAsync(string engagementAccount)
        {
            var messages = await ReportInProgressTableEntity.ListByAccountAsync(reportInProgressTable, engagementAccount);
            return messages?.Select(m => new MessageIdentifer(m.MessageId, m.CustomMessageId)).ToList();
        }

        public async Task<MessageRecord> GetMessageReportAsync(string engagementAccount, string messageId)
        {
            // Get from history table for metadata
            var historyTable = await GetHistoryTableAsync(engagementAccount);
            var historyEntity = await MessageHistoryTableEntity.GetAsync(historyTable, engagementAccount, messageId);
            if (historyEntity == null)
            {
                return null;
            }

            var record = new MessageRecord
            {
                MessageId = historyEntity.MessageId,
                SendTime = historyEntity.SendTime,
                Targets = historyEntity.Targets,
                Delivered = historyEntity.Delivered,
                Opened = historyEntity.Opened,
                Clicked = historyEntity.Clicked,
                CustomMessageId = historyEntity.CustomMessageId
            };

            // Try to get from in-progress table
            var inProgressEntity = await ReportInProgressTableEntity.GetAsync(reportInProgressTable, engagementAccount, messageId);
            if (inProgressEntity != null)
            {
                record.Targets = inProgressEntity.Targets;
                record.Delivered = inProgressEntity.Delivered;
                record.Opened = inProgressEntity.Opened;
                record.Clicked = inProgressEntity.Clicked;
                record.CustomMessageId = inProgressEntity.CustomMessageId;
            }

            return record;
        }

        public async Task OnAccountCreatedOrUpdatedAsync(string engagementAccount)
        {
            // Create history table if not exist
            await GetHistoryTableAsync(engagementAccount, true);

            // Init record in report lease table
            await ReportLeaseTableEntity.InitAccountAsync(reportLeaseTable, engagementAccount);
        }

        public async Task OnAccountDeletedAsync(string engagementAccount)
        {
            // Delete history table
            var historyTable = await GetHistoryTableAsync(engagementAccount);
            await historyTable.DeleteIfExistsAsync();

            // Delete record in report lease table
            await ReportLeaseTableEntity.DeleteAccountAsync(reportLeaseTable, engagementAccount);
        }

        private async Task<CloudTable> GetHistoryTableAsync(string engagementAccount, bool createIfNotExist = false)
        {
            var key = Regex.IsMatch(engagementAccount, @"^[a-zA-Z0-9]+$") ? engagementAccount : engagementAccount.GetHashCode().ToString();
            var tableName = string.Format(MessageHistoryTableName, key);
            var table = this.client.GetTableReference(tableName);

            if (createIfNotExist)
            {
                await table.CreateIfNotExistsAsync();
            }

            return table;
        }
    }
}
