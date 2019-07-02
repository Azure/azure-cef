// <copyright file="ReportLeaseTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    /// <summary>
    /// TableName: emailreportlease
    /// Usage: Lease handle for report to avoid concurrent pulling for same report
    /// Scope: Internal
    /// PartitionKey: EngagementAccount
    /// RowKey: EngagementAccount
    /// </summary>
    public class ReportLeaseTableEntity : TableEntity
    {
        public ReportLeaseTableEntity()
        {
        }

        public ReportLeaseTableEntity(string engagementAccount)
        {
            this.EngagementAccount = engagementAccount;

            this.PartitionKey = this.RowKey = this.EngagementAccount;
        }

        public string EngagementAccount { get; set; }

        public string Processor { get; set; }

        public DateTime? LastProcessTime { get; set; }

        public static async Task InitAccountAsync(CloudTable table, string engagementAccount)
        {
            var entity = new ReportLeaseTableEntity(engagementAccount);
            var operation = TableOperation.InsertOrMerge(entity);
            await table.ExecuteAsync(operation);
        }

        public static async Task DeleteAccountAsync(CloudTable table, string engagementAccount)
        {
            var entity = new ReportLeaseTableEntity(engagementAccount)
            {
                ETag = "*"
            };
            var operation = TableOperation.Delete(entity);
            await table.ExecuteAsync(operation);
        }

        public static async Task<List<ReportLeaseTableEntity>> ListAsync(CloudTable table, string processor = null)
        {
            TableContinuationToken token = null;

            var entities = new List<ReportLeaseTableEntity>();
            var tableQuery = new TableQuery<ReportLeaseTableEntity>();
            do
            {
                var queryResult = await table.ExecuteQuerySegmentedAsync(tableQuery, token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            }
            while (token != null);

            if (processor == null)
            {
                // Return entity with no lease
                return entities?.Where(e => string.IsNullOrEmpty(e.Processor)).ToList();
            }
            else
            {
                // Return entity locked by the processor
                return entities?.Where(e => e.Processor == processor).ToList();
            }
        }

        public static async Task<bool> TryAcquireLeaseAsync(CloudTable table, string engagementAccount, string processor)
        {
            try
            {
                // Get entity which contains Etag
                var tableQuery = TableOperation.Retrieve<ReportLeaseTableEntity>(engagementAccount, engagementAccount);
                var tableQueryResult = await table.ExecuteAsync(tableQuery);
                var entity = (ReportLeaseTableEntity)tableQueryResult?.Result;
                if (entity == null || !string.IsNullOrEmpty(entity.Processor))
                {
                    return false;
                }

                // Try to update processor
                entity.Processor = processor;
                entity.LastProcessTime = DateTime.UtcNow;

                // Azure Table by default uses optimistic concurrency checks as the default behavior for Replace
                var operation = TableOperation.Replace(entity);
                var result = await table.ExecuteAsync(operation);
                entity = (ReportLeaseTableEntity)result.Result;

                return true;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 412)
                {
                    EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, "ReportLeaseTableEntity", "TryAcquireLeaseAsync", OperationStates.Skipping, $"Processor {processor} - account {engagementAccount} is taken by others, skipped.");
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public static async Task<bool> TryReleaseLeaseAsync(CloudTable table, string engagementAccount, string processor)
        {
            try
            {
                // Get entity which contains Etag
                var tableQuery = TableOperation.Retrieve<ReportLeaseTableEntity>(engagementAccount, engagementAccount);
                var tableQueryResult = await table.ExecuteAsync(tableQuery);
                var entity = (ReportLeaseTableEntity)tableQueryResult?.Result;
                if (entity == null || string.IsNullOrEmpty(entity.Processor))
                {
                    return true;
                }

                // Try to update processor
                entity.Processor = null;

                // Azure Table by default uses optimistic concurrency checks as the default behavior for Replace
                var operation = TableOperation.Replace(entity);
                var result = await table.ExecuteAsync(operation);
                entity = (ReportLeaseTableEntity)result.Result;

                return true;
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 412)
                {
                    EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, "ReportLeaseTableEntity", "TryReleaseLeaseAsync", OperationStates.Skipping, $"Failed to release lease for Account {engagementAccount}. Processor {processor} will handle next round.");
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
