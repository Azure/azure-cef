// <copyright file="MessageBatchTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    /// <summary>
    /// TableName: smsbatch
    /// Usage: Tracking details for each push batch
    /// Scope: Internal
    /// PartitionKey: MessageId
    /// RowKey: BatchId
    /// </summary>
    public class MessageBatchTableEntity : TableEntity
    {
        public MessageBatchTableEntity()
            : base()
        {
        }

        public MessageBatchTableEntity(Guid messageId, Guid batchId)
        {
            this.MessageId = messageId.ToString();
            this.BatchId = batchId.ToString();

            this.PartitionKey = this.MessageId;
            this.RowKey = this.BatchId;
        }

        public MessageBatchTableEntity(OutputResult result)
            : this(result.MessageId, result.BatchId)
        {
            this.BatchSize = result.Targets?.Count ?? 0;
            this.State = $"outcome={result.DeliveryResponse.DeliveryOutcome}, delivered={result.Delivered}";
            this.LastUpdateTime = DateTime.UtcNow;
        }

        public string MessageId { get; set; }

        public string BatchId { get; set; }

        public int BatchSize { get; set; }

        public string State { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public static async Task InsertOrMergeAsync(CloudTable table, MessageBatchTableEntity entity)
        {
            var operation = TableOperation.InsertOrMerge(entity);
            await table.ExecuteAsync(operation);
        }
    }
}
