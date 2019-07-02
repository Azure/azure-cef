// <copyright file="MessageIdMappingTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    /// <summary>
    /// TableName: smsidmapping
    /// Usage: Some provider only return custom Id (message Id in provider's system) in report. Tracking the mapping for our MessageId (CEF) & CustomMessageId
    /// Scope: Internal
    /// PartitionKey: CustomMessageId
    /// RowKey: MessageId (CEF)
    /// </summary>
    public class MessageIdMappingTableEntity : TableEntity
    {
        public MessageIdMappingTableEntity()
            : base()
        {
        }

        public MessageIdMappingTableEntity(string messageId, string customMessagId)
        {
            this.CustomMessageId = customMessagId;
            this.MessageId = messageId;

            this.PartitionKey = this.CustomMessageId;
            this.RowKey = this.MessageId;
        }

        public MessageIdMappingTableEntity(string engagementAccount, string messageId, string customMessageId)
            : this(messageId, customMessageId)
        {
            this.EngagementAccount = engagementAccount;
        }

        public string CustomMessageId { get; set; }

        public string MessageId { get; set; }

        public string EngagementAccount { get; set; }

        public static async Task<MessageIdMappingTableEntity> GetAsync(CloudTable table, string customMessageId)
        {
            var tableQuery = new TableQuery<MessageIdMappingTableEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    customMessageId));

            var tableQueryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, null);
            return tableQueryResult?.Results?.SingleOrDefault();
        }

        public static async Task InsertOrMergeAsync(CloudTable table, MessageIdMappingTableEntity entity)
        {
            var operation = TableOperation.InsertOrMerge(entity);
            await table.ExecuteAsync(operation);
        }
    }
}
