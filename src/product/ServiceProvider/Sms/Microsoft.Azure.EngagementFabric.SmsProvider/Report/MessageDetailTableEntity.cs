// <copyright file="MessageDetailTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    /// <summary>
    /// TableName: smsdetail
    /// Usage: Tracking details for each sms report
    /// Scope: Internal & External
    /// PartitionKey: MessageId
    /// RowKey: PhoneNumber
    /// </summary>
    public class MessageDetailTableEntity : TableEntity
    {
        public MessageDetailTableEntity()
            : base()
        {
        }

        public MessageDetailTableEntity(string engagementAccount, string messageId, string phoneNumber)
        {
            this.EngagementAccount = engagementAccount;
            this.MessageId = messageId;
            this.PhoneNumber = phoneNumber;

            this.PartitionKey = this.MessageId;

            // For long message, we will ensure there're multiple detail records
            this.RowKey = $"{this.PhoneNumber}_{Guid.NewGuid()}";
        }

        public MessageDetailTableEntity(string engagementAccount, string messageId, ReportDetail report)
            : this(engagementAccount, messageId, report.PhoneNumber)
        {
            this.State = report.State.ToString();
            this.SubmitTime = report.SubmitTime;
            this.ReceiveTime = report.ReceiveTime;
            this.StateDetail = report.StateDetail;
        }

        public string MessageId { get; set; }

        public string EngagementAccount { get; set; }

        public string PhoneNumber { get; set; }

        public string State { get; set; }

        public string StateDetail { get; set; }

        public DateTime? SendTime { get; set; }

        public DateTime? SubmitTime { get; set; }

        public DateTime? ReceiveTime { get; set; }

        public static async Task<Tuple<int, int>> GetResultAsync(CloudTable table, string engagementAccount, string messageId)
        {
            var success = 0;
            var failed = 0;
            var tableQuery = new TableQuery<MessageDetailTableEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    messageId));

            TableContinuationToken token = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(tableQuery, token);
                token = segment.ContinuationToken;

                if (segment.Results != null)
                {
                    success += segment.Results.Count(r => r.State.Equals(MessageState.DELIVERED.ToString()));
                    failed += segment.Results.Count(r => !r.State.Equals(MessageState.DELIVERED.ToString()) && !r.State.Equals(MessageState.UNKNOWN.ToString()));
                }
            }
            while (token != null);

            return new Tuple<int, int>(success, failed);
        }

        public static async Task<MessageDetails> ListAsync(CloudTable table, string engagementAccount, string messageId, int count, TableContinuationToken continuationToken, MessageDetails result)
        {
            var tableQuery = new TableQuery<MessageDetailTableEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    messageId));

            tableQuery.TakeCount = count;

            var tableQueryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
            var details = tableQueryResult.Results?.Where(r => r.EngagementAccount.Equals(engagementAccount, StringComparison.OrdinalIgnoreCase)).ToList();
            result.Details = details?.Select(d => new MessageDetails.MessageDetailEntry
            {
                PhoneNumber = d.PhoneNumber,
                State = d.State,
                SubmitTime = d.SubmitTime,
                ReceiveTime = d.ReceiveTime
            }).ToList();

            result.ContinuationToken = tableQueryResult.ContinuationToken;
            return result;
        }

        public static async Task<IReadOnlyDictionary<string, int>> CountByStateAsync(
            CloudTable table,
            string messageId)
        {
            var countByState = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var tableQuery = new TableQuery<MessageDetailTableEntity>()
                .Where(
                    TableQuery.GenerateFilterCondition(
                        "PartitionKey",
                        QueryComparisons.Equal,
                        messageId));

            TableContinuationToken continuationToken = null;
            do
            {
                var segment = await table.ExecuteQuerySegmentedAsync(tableQuery, continuationToken);
                continuationToken = segment.ContinuationToken;

                foreach (var group in segment.Results.GroupBy(e => e.State))
                {
                    if (countByState.ContainsKey(group.Key))
                    {
                        countByState[group.Key] += group.Count();
                    }
                    else
                    {
                        countByState.Add(group.Key, group.Count());
                    }
                }
            }
            while (continuationToken != null);

            return countByState;
        }

        public static async Task InsertOrMergeBatchAsync(CloudTable table, List<MessageDetailTableEntity> records)
        {
            var groups = records
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
    }
}
