// <copyright file="ReportInProgressTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    /// <summary>
    /// TableName: emailreportinprogress
    /// Usage: Tracking email reports in  progress
    /// Scope: Internal
    /// PartitionKey: EngagementAccount
    /// RowKey: MessageId
    /// </summary>
    public class ReportInProgressTableEntity : TableEntity
    {
        public ReportInProgressTableEntity()
            : base()
        {
        }

        public ReportInProgressTableEntity(string engagementAccount, Guid messageId)
        {
            this.MessageId = messageId.ToString();
            this.EngagementAccount = engagementAccount;
            this.LastUpdateTime = DateTime.UtcNow;

            this.PartitionKey = this.EngagementAccount;
            this.RowKey = this.MessageId;
        }

        public ReportInProgressTableEntity(string engagementAccount, Guid messageId, string customMessageId)
            : this(engagementAccount, messageId)
        {
            this.CustomMessageId = customMessageId;
        }

        public string MessageId { get; set; }

        public string EngagementAccount { get; set; }

        public string CustomMessageId { get; set; }

        public int Targets { get; set; }

        public int Delivered { get; set; }

        public int Opened { get; set; }

        public int Clicked { get; set; }

        public DateTime LastUpdateTime { get; set; }

        public static async Task<ReportInProgressTableEntity> GetAsync(CloudTable table, string engagementAccount, string messageId)
        {
            var tableQuery = TableOperation.Retrieve<ReportInProgressTableEntity>(engagementAccount, messageId);
            var tableQueryResult = await table.ExecuteAsync(tableQuery);
            return (ReportInProgressTableEntity)tableQueryResult?.Result;
        }

        public static async Task<List<ReportInProgressTableEntity>> ListByAccountAsync(CloudTable table, string engagementAccount)
        {
            var tableQuery = new TableQuery<ReportInProgressTableEntity>().Where(
                TableQuery.GenerateFilterCondition(
                    "PartitionKey",
                    QueryComparisons.Equal,
                    engagementAccount));

            TableContinuationToken token = null;

            List<ReportInProgressTableEntity> entities = new List<ReportInProgressTableEntity>();
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            }
            while (token != null);

            return entities;
        }

        public static async Task<ReportInProgressTableEntity> InsertOrMergeAsync(CloudTable table, ReportInProgressTableEntity entity)
        {
            var operation = TableOperation.InsertOrMerge(entity);
            var result = await table.ExecuteAsync(operation);
            return (ReportInProgressTableEntity)result?.Result;
        }

        public static async Task<bool> TryUpdateAsync(CloudTable table, ReportInProgressTableEntity entity)
        {
            try
            {
                // Azure Table by default uses optimistic concurrency checks as the default behavior for Replace
                var operation = TableOperation.Replace(entity);
                var result = await table.ExecuteAsync(operation);
                entity = (ReportInProgressTableEntity)result.Result;

                return true;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 412)
                {
                    EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, "ReportInProgressTableEntity", "TryUpdateAsync", OperationStates.Skipping, $"Update entity failed due to conflict, skipped. account={entity.EngagementAccount} messageId={entity.MessageId}");
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task DeleteAsync(CloudTable table, ReportInProgressTableEntity entity)
        {
            var operation = TableOperation.Delete(entity);
            await table.ExecuteAsync(operation);
        }
    }
}
