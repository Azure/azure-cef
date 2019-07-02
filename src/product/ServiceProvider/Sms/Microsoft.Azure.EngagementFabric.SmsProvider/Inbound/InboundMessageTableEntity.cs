// <copyright file="InboundMessageTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Inbound
{
    public class InboundMessageTableEntity : TableEntity
    {
        public InboundMessageTableEntity()
            : base()
        {
        }

        public InboundMessageTableEntity(string engagementAccount, string signature, InboundMessage message, string extendedCode)
        {
            this.EngagementAccount = engagementAccount;
            this.Signature = signature;
            this.PhoneNumber = message.MoMessage?.PhoneNumber;
            this.ExtendedCode = extendedCode ?? message.MoMessage?.ExtendedCode;
            this.Message = message.MoMessage?.Content;
            this.InboundTime = message.MoMessage?.InboundTime;

            this.PartitionKey = this.EngagementAccount;
            this.RowKey = Guid.NewGuid().ToString();
        }

        public string EngagementAccount { get; set; }

        public string Signature { get; set; }

        public string PhoneNumber { get; set; }

        public string ExtendedCode { get; set; }

        public string Message { get; set; }

        public DateTime? InboundTime { get; set; }

        public static async Task InsertOrMergeBatchAsync(CloudTable table, List<InboundMessageTableEntity> entities)
        {
            var groups = entities
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 100)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            foreach (var group in groups)
            {
                var batchOperation = new TableBatchOperation();
                foreach (var entity in group)
                {
                    batchOperation.InsertOrMerge(entity);
                }

                await table.ExecuteBatchAsync(batchOperation);
            }
        }

        public static async Task<List<InboundMessageTableEntity>> ListAsync(CloudTable table, string engagementAccount, DateTime startTime, int count)
        {
            var partitionFilter = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.Equal,
                engagementAccount);

            var dateFilter = TableQuery.GenerateFilterConditionForDate(
                "InboundTime",
                QueryComparisons.GreaterThanOrEqual,
                startTime);

            var filter = TableQuery.CombineFilters(partitionFilter, TableOperators.And, dateFilter);

            var tableQuery = new TableQuery<InboundMessageTableEntity>().Where(filter);
            tableQuery.TakeCount = count;

            var tableQueryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, null);
            return tableQueryResult.Results;
        }

        public static async Task DeleteAsync(CloudTable table, List<InboundMessageTableEntity> messages)
        {
            var batchOperation = new TableBatchOperation();
            foreach (var entity in messages)
            {
                batchOperation.Delete(entity);
            }

            await table.ExecuteBatchAsync(batchOperation);
        }

        public static async Task DeleteAsync(CloudTable table, string engagementAccount)
        {
            var filter = TableQuery.GenerateFilterCondition(
                "PartitionKey",
                QueryComparisons.Equal,
                engagementAccount);

            await CleanupAsync(table, filter);
        }

        public static async Task CleanupAsync(CloudTable table, DateTime before)
        {
            var dateFilter = TableQuery.GenerateFilterConditionForDate(
                "InboundTime",
                QueryComparisons.LessThan,
                before);

            await CleanupAsync(table, dateFilter);
        }

        private static async Task CleanupAsync(CloudTable table, string filter)
        {
            TableContinuationToken token = null;
            do
            {
                var tableQuery = new TableQuery<InboundMessageTableEntity>().Where(filter);
                var tableQueryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, token);
                if (tableQueryResult.Results != null && tableQueryResult.Results.Count > 0)
                {
                    SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, "InboudMessageTableEntity", "CleanupAsync", OperationStates.Committing, $"Clean up {tableQueryResult.Results.Count} inbound messages.");
                    TaskHelper.FireAndForget(() => DeleteAsync(table, tableQueryResult.Results));
                }

                token = tableQueryResult.ContinuationToken;
            }
            while (token != null);
        }
    }
}
