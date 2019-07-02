// <copyright file="MessageAuditTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    /// <summary>
    /// TableName: emailaudit
    /// Usage: Tracking the email sending summary. Also used for billing audit
    /// Scope: Internal
    /// PartitionKey: EngagementAccount
    /// RowKey: MessageId
    /// </summary>
    public class MessageAuditTableEntity : TableEntity
    {
        public MessageAuditTableEntity()
            : base()
        {
        }

        public MessageAuditTableEntity(MessageHistoryTableEntity history)
        {
            this.EngagementAccount = history.EngagementAccount;
            this.MessageId = history.MessageId;
            this.MessageBody = history.MessageBody;
            this.SendTime = history.SendTime;
            this.LastUpdateTime = history.LastUpdateTime;

            this.PartitionKey = this.EngagementAccount;
            this.RowKey = this.MessageId;
        }

        public string MessageId { get; set; }

        public string EngagementAccount { get; set; }

        public string MessageBody { get; set; }

        public DateTime? SendTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public static async Task InsertOrMergeAsync(CloudTable table, MessageAuditTableEntity entity)
        {
            var operation = TableOperation.InsertOrMerge(entity);
            await table.ExecuteAsync(operation);
        }
    }
}
