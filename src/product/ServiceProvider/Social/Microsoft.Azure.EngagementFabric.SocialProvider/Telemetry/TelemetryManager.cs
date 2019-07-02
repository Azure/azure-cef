// <copyright file="TelemetryManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.SocialProvider.Store;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Telemetry
{
    public class TelemetryManager
    {
        public async Task CreateSocialLoginHistoryRecordAsync(StoreAgent storeAgent, string account, string channel, string channelId, string platform, string action, DateTime timeStart)
        {
            var entity = new SocialLoginHistoryTableEntity(account, channel, channelId, platform, action, timeStart);

            var client = storeAgent.StorageAccount.CreateCloudTableClient();

            // Tablename change with account
            var tablename = StoreManager.UserInfoHistoryTableName + account;
            var table = client.GetTableReference(tablename);
            await SocialLoginHistoryTableEntity.InsertSocialLoginHistoryTableEntity(table, entity);
        }

        public async Task DeleteSocialLoginHistoryDataAsync(StoreAgent storeAgent)
        {
            var client = storeAgent.StorageAccount.CreateCloudTableClient();

            // Delete history data 6 months ago
            int monthCount = 6;
            var startTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month - monthCount, 1).Ticks.ToString();
            try
            {
                var tableList = client.ListTables(StoreManager.UserInfoHistoryTableName);
                if (tableList != null)
                {
                    foreach (var table in tableList.ToList())
                    {
                        if (await table.ExistsAsync())
                        {
                            var projectionQuery = new TableQuery()
                              .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThan, startTime))
                              .Select(new[] { "RowKey" });

                            var entities = table.ExecuteQuery(projectionQuery).ToList();
                            var offset = 0;
                            while (offset < entities.Count)
                            {
                                var batch = new TableBatchOperation();
                                var rows = entities.Skip(offset).Take(100).ToList();
                                foreach (var row in rows)
                                {
                                    batch.Delete(row);
                                }

                                await table.ExecuteBatchAsync(batch);
                                offset += rows.Count;
                            }
                        }
                    }
                }

                SocialProviderEventSource.Current.Info(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteSocialLoginHistoryDataAsync), OperationStates.Succeeded, string.Empty);
            }
            catch (Exception ex)
            {
                SocialProviderEventSource.Current.ErrorException(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteSocialLoginHistoryDataAsync), OperationStates.Failed, "Failed to delete history data in storage table", ex);
            }
        }

        public async Task CreateSocialLoginAccountAsync(StoreAgent storeAgent, string account)
        {
            var client = storeAgent.StorageAccount.CreateCloudTableClient();
            var tablename = StoreManager.UserInfoHistoryTableName + account;
            var table = client.GetTableReference(tablename);
            await table.CreateIfNotExistsAsync();
        }

        public async Task DeleteSocialLoginAccount(StoreAgent storeAgent, string account)
        {
            var client = storeAgent.StorageAccount.CreateCloudTableClient();
            var tablename = StoreManager.UserInfoHistoryTableName + account;
            var table = client.GetTableReference(tablename);
            var tableDelete = table.DeleteIfExistsAsync();
            await Task.CompletedTask;
        }
    }
}
