// <copyright file="BillingUsageBucket.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Microsoft.Azure.EngagementFabric.BillingService.Manager
{
    public class BillingUsageBucket
    {
        private string bucketKey;
        private IReliableDictionary<string, ResourceUsageRecord> bucket;
        private IReliableStateManager stateManager;

        public BillingUsageBucket(IReliableStateManager stateManager, string bucketKey, BillingUsageBucketState state)
        {
            this.stateManager = stateManager;
            this.bucketKey = bucketKey;
            this.State = state;

            this.bucket = stateManager.GetOrAddAsync<IReliableDictionary<string, ResourceUsageRecord>>(bucketKey).Result;
        }

        public string BucketKey => this.bucketKey;

        public BillingUsageBucketState State { get; set; }

        public async Task<long> GetSizeAsync(CancellationToken cancellationToken)
        {
            using (var tx = this.stateManager.CreateTransaction())
            {
                return await this.bucket.GetCountAsync(tx);
            }
        }

        public async Task AddRecordsAsync(IEnumerable<ResourceUsageRecord> records, CancellationToken cancellationToken)
        {
            using (var tx = this.stateManager.CreateTransaction())
            {
                foreach (var record in records)
                {
                    await this.bucket.AddAsync(tx, Guid.NewGuid().ToString(), record);
                }

                await tx.CommitAsync();
            }
        }

        public async Task<List<ResourceUsageRecord>> GetRecordsAsync(CancellationToken cancellationToken)
        {
            var records = new Dictionary<string, ResourceUsageRecord>();
            using (var tx = this.stateManager.CreateTransaction())
            {
                var enumerable = await this.bucket.CreateEnumerableAsync(tx);
                var enumerator = enumerable.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(cancellationToken))
                {
                    var record = enumerator.Current.Value;
                    var key = $"{record.EngagementAccount}:{record.UsageType}";
                    if (records.ContainsKey(key))
                    {
                        records[key].Quantity += record.Quantity;
                    }
                    else
                    {
                        records.Add(key, new ResourceUsageRecord(record));
                    }
                }
            }

            return records.Select(r => r.Value).ToList();
        }

        public async Task ClearAsync()
        {
            await this.bucket.ClearAsync();
        }
    }
}
