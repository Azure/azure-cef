// <copyright file="MessageHistoryTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    /// <summary>
    /// TableName: emailhistory
    /// Usage: Tracking the email sending history
    /// Scope: External
    /// PartitionKey: MessageId
    /// RowKey: EngagementAccount
    public class MessageHistoryTableEntity : TableEntity
    {
        public MessageHistoryTableEntity()
            : base()
        {
        }

        public MessageHistoryTableEntity(string engagementAccount, string messageId)
        {
            this.EngagementAccount = engagementAccount;
            this.MessageId = messageId;
            this.LastUpdateTime = DateTime.UtcNow;

            this.PartitionKey = this.MessageId;
            this.RowKey = this.EngagementAccount;
        }

        public MessageHistoryTableEntity(InputMessage message, EmailMessageInfoExtension extension)
            : this(message.MessageInfo.EngagementAccount, message.MessageInfo.MessageId.ToString())
        {
            this.MessageBody = message.MessageInfo.MessageBody;
            this.SendTime = message.MessageInfo.SendTime;
        }

        public MessageHistoryTableEntity(ReportInProgressTableEntity entity)
            : this(entity.EngagementAccount, entity.MessageId)
        {
            this.CustomMessageId = entity.CustomMessageId;
            this.Targets = entity.Targets;
            this.Delivered = entity.Delivered;
            this.Opened = entity.Opened;
            this.Clicked = entity.Clicked;
        }

        public string MessageId { get; set; }

        public string CustomMessageId { get; set; }

        public string EngagementAccount { get; set; }

        public string MessageBody { get; set; }

        public int Targets { get; set; }

        public int Delivered { get; set; }

        public int Opened { get; set; }

        public int Clicked { get; set; }

        public DateTime? SendTime { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public static async Task<MessageHistoryTableEntity> GetAsync(CloudTable table, string engagementAccount, string messageId)
        {
            var tableQuery = TableOperation.Retrieve<MessageHistoryTableEntity>(messageId, engagementAccount);
            var tableQueryResult = await table.ExecuteAsync(tableQuery);
            return (MessageHistoryTableEntity)tableQueryResult?.Result;
        }

        public static async Task InsertOrMergeAsync(CloudTable table, MessageHistoryTableEntity entity)
        {
            var operation = TableOperation.InsertOrMerge(entity);
            await table.ExecuteAsync(operation);
        }
    }
}
