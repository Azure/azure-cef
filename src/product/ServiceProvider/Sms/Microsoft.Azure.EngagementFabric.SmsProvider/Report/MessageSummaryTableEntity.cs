// <copyright file="MessageSummaryTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    /// <summary>
    /// TableName: smssummary
    /// Usage: Tracking the sms sending summary. Also used for billing audit
    /// Scope: Internal
    /// PartitionKey: MessageId
    /// RowKey: EngagementAccount
    /// </summary>
    public class MessageSummaryTableEntity : TableEntity
    {
        public MessageSummaryTableEntity()
            : base()
        {
        }

        public MessageSummaryTableEntity(InputMessage message, SmsMessageInfoExtension extension)
        {
            this.EngagementAccount = message.MessageInfo.EngagementAccount;
            this.MessageId = message.MessageInfo.MessageId.ToString();
            this.MessageBody = message.MessageInfo.MessageBody;
            this.Targets = message.Targets.Count;
            this.SendTime = message.MessageInfo.SendTime;
            this.LastUpdateTime = DateTime.UtcNow;
            this.MessageCategory = extension.MessageCategory.ToString();

            this.PartitionKey = this.MessageId;
            this.RowKey = this.EngagementAccount;
        }

        public string MessageId { get; set; }

        public string EngagementAccount { get; set; }

        public string MessageBody { get; set; }

        public int Targets { get; set; }

        public DateTime SendTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public string MessageCategory { get; set; }

        public int? Units { get; set; }

        public static async Task<MessageSummaryTableEntity> GetAsync(CloudTable table, string messageId)
        {
            var tableQuery = new TableQuery<MessageSummaryTableEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    messageId));

            var tableQueryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, null);
            return tableQueryResult?.Results?.FirstOrDefault();
        }

        public static async Task InsertOrMergeAsync(CloudTable table, MessageSummaryTableEntity entity)
        {
            var operation = TableOperation.InsertOrMerge(entity);
            await table.ExecuteAsync(operation);
        }
    }
}
