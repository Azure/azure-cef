// <copyright file="MessageHistoryTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    /// <summary>
    /// TableName: smshistory
    /// Usage: Tracking the sms sending history
    /// Scope: External
    /// PartitionKey: MessageId
    /// RowKey: EngagementAccount
    public class MessageHistoryTableEntity : TableEntity
    {
        public const string SendTimePartitionKeyMin = "T000000000000";
        public const string SendTimePartitionKeyMax = "T099999999999";

        public MessageHistoryTableEntity()
            : base()
        {
        }

        public MessageHistoryTableEntity(MessageSummaryTableEntity entity, bool useSendTimeAsPartitionKey)
        {
            this.EngagementAccount = entity.EngagementAccount;
            this.MessageId = entity.MessageId;
            this.MessageBody = entity.MessageBody;
            this.Targets = entity.Targets;
            this.SendTime = entity.SendTime;
            this.LastUpdateTime = entity.LastUpdateTime;
            this.MessageCategory = entity.MessageCategory;

            if (useSendTimeAsPartitionKey)
            {
                this.PartitionKey = ToPartitionKey(new DateTimeOffset(this.SendTime));
                this.RowKey = this.MessageId;
            }
            else
            {
                this.PartitionKey = this.MessageId;
                this.RowKey = this.EngagementAccount;
            }
        }

        public string MessageId { get; set; }

        public string EngagementAccount { get; set; }

        public string MessageBody { get; set; }

        public int Targets { get; set; }

        public DateTime SendTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string MessageCategory { get; set; }

        public static async Task<MessageHistoryTableEntity> GetAsync(CloudTable table, string engagementAccount, string messageId)
        {
            var tableQuery = TableOperation.Retrieve<MessageHistoryTableEntity>(messageId, engagementAccount);
            var tableQueryResult = await table.ExecuteAsync(tableQuery);

            if (tableQueryResult.Result != null)
            {
                return (MessageHistoryTableEntity)tableQueryResult.Result;
            }

            // Backward compatible
            tableQuery = TableOperation.Retrieve<MessageHistoryTableEntity>(engagementAccount, messageId);
            tableQueryResult = await table.ExecuteAsync(tableQuery);
            return (MessageHistoryTableEntity)tableQueryResult.Result;
        }

        public static async Task InsertOrMergeAsync(CloudTable table, MessageSummaryTableEntity summaryEntity)
        {
            var operations = new[]
            {
                TableOperation.InsertOrMerge(new MessageHistoryTableEntity(summaryEntity, false)),
                TableOperation.InsertOrMerge(new MessageHistoryTableEntity(summaryEntity, true))
            };

            var tasks = operations.Select(operation => table.ExecuteAsync(operation));
            await Task.WhenAll(tasks);
        }

        public static async Task<TableQuerySegment<MessageHistoryTableEntity>> GetAsync(
            CloudTable table,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int count,
            TableContinuationToken continuationToken)
        {
            var query = new TableQuery<MessageHistoryTableEntity>()
                .Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, ToPartitionKey(startTime)),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, ToPartitionKey(endTime))))
                .Take(count);

            return await table.ExecuteQuerySegmentedAsync(query, continuationToken);
        }

        public static string ToPartitionKey(DateTimeOffset timeOffset)
        {
            return $"T{timeOffset.ToUnixTimeSeconds():d012}";
        }
    }
}
