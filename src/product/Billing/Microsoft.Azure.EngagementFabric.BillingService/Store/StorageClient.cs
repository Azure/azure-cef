// <copyright file="StorageClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.BillingService.Store
{
    public class StorageClient
    {
        private readonly CloudStorageAccount storageAccount;

        public StorageClient(string connectionString)
        {
            this.storageAccount = CloudStorageAccount.Parse(connectionString);
        }

        public CloudTable GetTable(string tableName)
        {
            var tableClient = this.storageAccount.CreateCloudTableClient();
            var tableOptions = new TableRequestOptions
            {
                LocationMode = LocationMode.PrimaryOnly,
                MaximumExecutionTime = TimeSpan.FromSeconds(30),
                RetryPolicy = new ExponentialRetry(deltaBackoff: TimeSpan.FromSeconds(1), maxAttempts: 7),
                ServerTimeout = TimeSpan.FromSeconds(30)
            };
            tableClient.DefaultRequestOptions = tableOptions;
            return tableClient.GetTableReference(tableName);
        }

        public CloudQueue GetQueue(string queueName)
        {
            CloudQueueClient queueClient = this.storageAccount.CreateCloudQueueClient();
            var queueOptions = new QueueRequestOptions
            {
                LocationMode = LocationMode.PrimaryOnly,
                MaximumExecutionTime = TimeSpan.FromSeconds(30),
                RetryPolicy = new ExponentialRetry(deltaBackoff: TimeSpan.FromSeconds(1), maxAttempts: 7),
                ServerTimeout = TimeSpan.FromSeconds(30)
            };
            queueClient.DefaultRequestOptions = queueOptions;
            return queueClient.GetQueueReference(queueName);
        }
    }
}
