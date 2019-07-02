// <copyright file="TelemetryManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Telemetry
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    public class TelemetryManager
    {
        // Telemetry storage table name
        public const string OtpCodeHistoryTableName = "otpcodehistory";

        private CloudStorageAccount storageAccount;

        public TelemetryManager(string connectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public async Task CreateOtpCodeHistoryRecord(string account, string phoneNumber, string action, DateTime timeStart)
        {
            var entity = new OtpHistoryTableEntity(account, phoneNumber, action, timeStart);

            var client = this.storageAccount.CreateCloudTableClient();

            // tablename change with account
            var tablename = OtpCodeHistoryTableName + account;
            var table = client.GetTableReference(tablename);
            await OtpHistoryTableEntity.InsertOtpLoginHistoryTableEntity(table, entity);
        }

        public async Task DeleteOtpCodeHistoryDataAsync()
        {
            var client = this.storageAccount.CreateCloudTableClient();

            // Delete history data 6 months ago
            int monthCount = 6;
            var startTime = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month - monthCount, 1).Ticks.ToString();
            try
            {
                var tableList = client.ListTables(OtpCodeHistoryTableName);
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

                OtpProviderEventSource.Current.Info(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteOtpCodeHistoryDataAsync), OperationStates.Succeeded, string.Empty);
            }
            catch (Exception ex)
            {
                OtpProviderEventSource.Current.ErrorException(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteOtpCodeHistoryDataAsync), OperationStates.Failed, "Failed to delete history data in storage table", ex);
            }
        }

        public async Task CreateOtpAccountAsync(string account)
        {
            var client = this.storageAccount.CreateCloudTableClient();
            var tablename = OtpCodeHistoryTableName + account;
            var table = client.GetTableReference(tablename);
            await table.CreateIfNotExistsAsync();
        }

        public async Task DeleteOtpAccount(string account)
        {
            var client = this.storageAccount.CreateCloudTableClient();
            var tablename = OtpCodeHistoryTableName + account;
            var table = client.GetTableReference(tablename);
            var tableDelete = table.DeleteIfExistsAsync();
            await Task.CompletedTask;
        }
    }
}