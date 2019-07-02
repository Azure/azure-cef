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
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Billing;
using Microsoft.Azure.EngagementFabric.SmsProvider.Configuration;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Store;
using Microsoft.Azure.EngagementFabric.SmsProvider.Utils;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    public class ReportTelemetryManager : IReportTelemetryManager
    {
        // Internal tables
        private static readonly string MessageSummaryTableName = "smssummary";
        private static readonly string MessageDetailTableName = "smsdetail";
        private static readonly string MessageBatchRecordTableName = "smsbatch";
        private static readonly string MessageIdMappingTableName = "smsidmapping";

        // Customer-facing tables
        private static readonly string MessageHistoryTableName = "smshistory{0}";

        private CloudTableClient client;
        private BillingAgent billingAgent;
        private MetricManager metricManager;

        private CloudTable summaryTable;
        private CloudTable detailTable;
        private CloudTable batchTable;
        private CloudTable idMappingTable;
        private SemaphoreSlim updateLock;

        private ISmsStore store;
        private ICredentialManager credentialManager;

        public ReportTelemetryManager(
            ISmsStoreFactory factory,
            ServiceConfiguration configuration,
            BillingAgent billingAgent,
            MetricManager metricManager,
            ICredentialManager credentialManager)
        {
            this.store = factory.GetStore();

            var account = CloudStorageAccount.Parse(configuration.TelemetryStoreConnectionString);
            this.client = account.CreateCloudTableClient();

            this.summaryTable = this.client.GetTableReference(MessageSummaryTableName);
            this.detailTable = this.client.GetTableReference(MessageDetailTableName);
            this.batchTable = this.client.GetTableReference(MessageBatchRecordTableName);
            this.idMappingTable = this.client.GetTableReference(MessageIdMappingTableName);

            this.summaryTable.CreateIfNotExists();
            this.detailTable.CreateIfNotExists();
            this.batchTable.CreateIfNotExists();
            this.idMappingTable.CreateIfNotExists();

            this.billingAgent = billingAgent;
            this.metricManager = metricManager;
            this.credentialManager = credentialManager;

            this.updateLock = new SemaphoreSlim(1, 1);
        }

        public async Task OnMessageSentAsync(InputMessage message, SmsMessageInfoExtension extension)
        {
            // Create record in summary table
            var summary = new MessageSummaryTableEntity(message, extension);
            await MessageSummaryTableEntity.InsertOrMergeAsync(this.summaryTable, summary);

            // Create record in history table
            var historyTable = await this.GetHistoryTableAsync(message.MessageInfo.EngagementAccount);
            if (historyTable == null)
            {
                // Create the table in case any issue in account initialize
                historyTable = await this.GetHistoryTableAsync(message.MessageInfo.EngagementAccount, true);
            }

            await MessageHistoryTableEntity.InsertOrMergeAsync(historyTable, summary);
        }

        public async Task<bool> OnMessageDispatchedAsync(string engagementAccount, string messageId, string customMessageId, List<string> targets, ConnectorIdentifier connector)
        {
            if (!string.IsNullOrEmpty(customMessageId))
            {
                // Insert id mapping
                await MessageIdMappingTableEntity.InsertOrMergeAsync(this.idMappingTable, new MessageIdMappingTableEntity(engagementAccount, messageId, customMessageId));
            }

            // Get record from summary table
            var entity = await MessageSummaryTableEntity.GetAsync(this.summaryTable, messageId);
            if (entity == null)
            {
                SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnMessageReportUpdatedAsync), OperationStates.Dropped, $"Does not find record for account={engagementAccount} messageId={messageId}");
                return false;
            }

            // Calculating the unit according to provider
            var metadata = await this.credentialManager.GetMetadata(connector.ConnectorName);
            var units = BillingHelper.CalculateBillingUnits(entity.MessageBody, metadata);
            if (entity.Units == null)
            {
                entity.Units = units;
                entity.LastUpdateTime = DateTime.UtcNow;
                await MessageSummaryTableEntity.InsertOrMergeAsync(this.summaryTable, entity);
            }

            return true;
        }

        public async Task<bool> OnMessageReportUpdatedAsync(string messageId, string customMessageId, List<ReportDetail> reports, ConnectorIdentifier connector)
        {
            if (string.IsNullOrEmpty(messageId) && !string.IsNullOrEmpty(customMessageId))
            {
                // Get id mapping, retry 3 times if cannot found (in case racing with case #2 who has not insert the mapping yet)
                int retry = 3;
                while (retry > 0)
                {
                    messageId = (await MessageIdMappingTableEntity.GetAsync(idMappingTable, customMessageId))?.MessageId;
                    if (!string.IsNullOrEmpty(messageId))
                    {
                        break;
                    }

                    retry--;
                    await Task.Delay(1000);
                }

                if (string.IsNullOrEmpty(messageId))
                {
                    SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnMessageReportUpdatedAsync), OperationStates.Dropped, $"Does not find record for customMessageId={customMessageId}");
                    return false;
                }
            }

            // Get record from summary table
            var entity = await MessageSummaryTableEntity.GetAsync(this.summaryTable, messageId);
            if (entity == null)
            {
                SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnMessageReportUpdatedAsync), OperationStates.Dropped, $"Does not find record for messageId={messageId}");
                return false;
            }

            // Calculating the unit according to provider
            // This is for long message that will be charged as multiple messages
            var metadata = await this.credentialManager.GetMetadata(connector.ConnectorName);
            var units = BillingHelper.CalculateBillingUnits(entity.MessageBody, metadata);

            // Report might be ealier than dispatch result, so make sure units is updated
            if (entity.Units == null)
            {
                entity.Units = units;
                entity.LastUpdateTime = DateTime.UtcNow;
                await MessageSummaryTableEntity.InsertOrMergeAsync(this.summaryTable, entity);
            }

            // Update detail table
            if (reports != null && reports.Count > 0)
            {
                await this.UpdateMessageDetailsAsync(entity.EngagementAccount, entity, reports);
            }

            // Get account detail
            var account = await this.store.GetAccountAsync(entity.EngagementAccount);
            var succeed = reports.Where(r => r.State == MessageState.DELIVERED).Count();
            var failed = reports.Where(r => r.State != MessageState.DELIVERED && r.State != MessageState.UNKNOWN).Count();

            // Push metric and billing usage
            if (Enum.TryParse(entity.MessageCategory, out MessageCategory category))
            {
                if (succeed > 0)
                {
                    this.metricManager.LogDeliverSuccess(succeed * units, entity.EngagementAccount, account?.SubscriptionId ?? string.Empty, category);

                    if (Constants.MessageCategoryToUsageTypeMappings.TryGetValue(category, out ResourceUsageType usageType))
                    {
                        var usage = new ResourceUsageRecord();
                        usage.EngagementAccount = entity.EngagementAccount;
                        usage.UsageType = usageType;
                        usage.Quantity = succeed * units;

                        await this.billingAgent.StoreBillingUsageAsync(new List<ResourceUsageRecord> { usage }, CancellationToken.None);

                        SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnMessageReportUpdatedAsync), OperationStates.Succeeded, $"Usage pushed to billing service account={entity.EngagementAccount} usageType={usageType} quantity={succeed}x{units}");
                    }
                }

                if (failed > 0)
                {
                    this.metricManager.LogDeliverFailed(failed * units, entity.EngagementAccount, account?.SubscriptionId ?? string.Empty, category);
                }
            }
            else
            {
                SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnMessageReportUpdatedAsync), OperationStates.Failed, $"Failed to parse MessageCategory={entity.MessageCategory} MessageId={entity.MessageId}");
            }

            return true;
        }

        public async Task<MessageDetails> GetMessageHistoryAsync(string engagementAccount, string messageId, int count, TableContinuationToken continuationToken)
        {
            var historyTable = await this.GetHistoryTableAsync(engagementAccount);
            var record = await MessageHistoryTableEntity.GetAsync(historyTable, engagementAccount, messageId);
            if (record == null)
            {
                return null;
            }

            var result = new MessageDetails
            {
                MessageId = record.MessageId,
                SendTime = record.SendTime,
                Targets = record.Targets
            };

            // Calculate success and failed count
            var tuple = await MessageDetailTableEntity.GetResultAsync(this.detailTable, engagementAccount, messageId);
            result.Succeed = tuple.Item1;
            result.Failed = tuple.Item2;

            // Get details
            return await MessageDetailTableEntity.ListAsync(this.detailTable, engagementAccount, messageId, count, continuationToken, result);
        }

        public async Task<PerMessageAggregationList> GetPerMessageAggregationAsync(
            string engagementAccount,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int count,
            TableContinuationToken continuationToken)
        {
            var historyTable = await this.GetHistoryTableAsync(engagementAccount);
            var segment = await MessageHistoryTableEntity.GetAsync(
                historyTable,
                startTime,
                endTime,
                count,
                continuationToken);

            var aggregations = await Task.WhenAll(
                segment.Select(async historyEntity =>
                {
                    var countByState = await MessageDetailTableEntity.CountByStateAsync(
                        this.detailTable,
                        historyEntity.MessageId);

                    return new PerMessageAggregationList.PerMessageAggregation(
                        historyEntity,
                        countByState);
                }));

            return new PerMessageAggregationList(
                aggregations,
                startTime,
                endTime,
                segment.ContinuationToken);
        }

        public async Task CreateMessageHistoryIfNotExistAsync(string engagementAccount)
        {
            await this.GetHistoryTableAsync(engagementAccount, true);
        }

        public async Task DeleteMessageHistoryAsync(string engagementAccount)
        {
            var historyTable = await this.GetHistoryTableAsync(engagementAccount);
            await historyTable.DeleteIfExistsAsync();
        }

        public async Task InsertMessageBatchRecordAsync(OutputResult result)
        {
            var table = this.client.GetTableReference(MessageBatchRecordTableName);
            await MessageBatchTableEntity.InsertOrMergeAsync(table, new MessageBatchTableEntity(result));
        }

        private async Task UpdateMessageDetailsAsync(string engagementAccount, MessageSummaryTableEntity summary, List<ReportDetail> reports)
        {
            var entities = new List<MessageDetailTableEntity>();
            var units = summary.Units ?? 1;
            foreach (var report in reports)
            {
                // Duplicate detail record if units is not 1
                for (var i = 0; i < units; i++)
                {
                    entities.Add(new MessageDetailTableEntity(engagementAccount, summary.MessageId, report));
                }
            }

            await MessageDetailTableEntity.InsertOrMergeBatchAsync(this.detailTable, entities);
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
