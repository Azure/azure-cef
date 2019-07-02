// <copyright file="BillingManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Billing.Common;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;
using Microsoft.Azure.EngagementFabric.BillingService.Configuration;
using Microsoft.Azure.EngagementFabric.BillingService.Store;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.BillingService.Manager
{
    public class BillingManager : BillingStore
    {
        private const int MaxBucketCount = 5;
        private const string BucketStatusKeyName = "BucketStates";
        private const string BucketKeyTemplate = "Bucket_{0}";
        private static readonly TimeSpan StateOperationTimeout = TimeSpan.FromSeconds(4);
        private static readonly TimeSpan HeartBeatInterval = TimeSpan.FromHours(1);
        private static readonly ReadOnlyTenantCacheClient TenantCacheClient = ReadOnlyTenantCacheClient.GetClient(true);

        private CloudTable table;
        private CloudQueue queue;
        private bool pushUsageForWhitelistedSubscriptionsOnly;
        private IReadOnlyList<Guid> whitelistedSubscriptionIds;

        private IReliableStateManager stateManager;
        private Dictionary<string, BillingUsageBucket> buckets;

        private ReaderWriterLockSlim bucketStateLock;
        private DateTime lastHeartBeat;

        public BillingManager(
            ServiceConfiguration configuration,
            IReliableStateManager stateManager)
            : base(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(5), 100, 10)
        {
            var storageClient = new StorageClient(configuration.StoreAccountConnectionString);
            this.table = storageClient.GetTable(configuration.UsageReportingTableName);
            this.queue = storageClient.GetQueue(configuration.UsageReportingQueueName);

            this.pushUsageForWhitelistedSubscriptionsOnly = configuration.PushUsageForWhitelistedSubscriptionsOnly;
            this.whitelistedSubscriptionIds = configuration.WhitelistedSubscriptions;
            this.stateManager = stateManager;

            this.buckets = new Dictionary<string, BillingUsageBucket>();
            this.bucketStateLock = new ReaderWriterLockSlim();

            this.lastHeartBeat = DateTime.MinValue;
        }

        public override async Task OnRunAsync(CancellationToken cancellationToken)
        {
            BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.OnRunAsync), OperationStates.Starting, "Billing manager OnRunAsync started");
            await LoadBucketsAsync(cancellationToken);
            await base.OnRunAsync(cancellationToken);
            BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.OnRunAsync), OperationStates.Empty, "Billing manager OnRunAsync stopped due to cancellation");
        }

        public override async Task StoreBillingUsageAsync(IEnumerable<ResourceUsageRecord> records, CancellationToken cancellationToken)
        {
            BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.StoreBillingUsageAsync), OperationStates.Received, $"Billing manager received {records.Count()} usage record(s)");
            var bucket = await GetOpenBucketAsync(cancellationToken);
            await bucket.AddRecordsAsync(records, cancellationToken);
            BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.StoreBillingUsageAsync), OperationStates.Succeeded, $"Billing manager stored {records.Count()} usage record(s) in bucket {bucket.BucketKey}");
        }

        protected override async Task PushUsageInnerLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                PrintBucketState();
                ApplicationException exception = null;

                var bucket = await GetOpenBucketAsync(cancellationToken);
                if (await bucket.GetSizeAsync(cancellationToken) > 0)
                {
                    var nextBucket = await OpenNextBucketAsync(bucket.BucketKey, true, cancellationToken);

                    // Wait for 10s to ensure all in-flight requests are done
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    if (await UpdateBucketStateAsync(bucket.BucketKey, BillingUsageBucketState.Open, BillingUsageBucketState.Sending, cancellationToken))
                    {
                        if (await PushBucketAsync(bucket, cancellationToken))
                        {
                            await UpdateBucketStateAsync(bucket.BucketKey, BillingUsageBucketState.Sending, BillingUsageBucketState.Idle, cancellationToken);
                        }
                        else
                        {
                            await UpdateBucketStateAsync(bucket.BucketKey, BillingUsageBucketState.Sending, BillingUsageBucketState.Fault, cancellationToken);
                            exception = new ApplicationException($"Failed to push bucket '{bucket.BucketKey}'");
                            throw exception;
                        }
                    }
                }
                else
                {
                    BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.PushUsageInnerLoopAsync), OperationStates.Dropped, $"No usage data. Ignore push. Current bucket is {bucket.BucketKey}");
                }
            }
            catch (Exception ex)
            {
                BillingEventSource.Current.ErrorException(BillingEventSource.EmptyTrackingId, this, nameof(this.PushUsageInnerLoopAsync), OperationStates.Failed, string.Empty, ex);
            }

            // Push heart beat
            try
            {
                if ((this.lastHeartBeat + HeartBeatInterval) <= DateTime.UtcNow)
                {
                    await this.NotifyPushAsync(Guid.Empty, cancellationToken);
                    BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, "Heartbeat", OperationStates.Empty, "Heart beat sent");

                    this.lastHeartBeat = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                BillingEventSource.Current.ErrorException(BillingEventSource.EmptyTrackingId, this, "Heartbeat", OperationStates.Failed, string.Empty, ex);
            }
        }

        protected override async Task PushToTargetAsync(IEnumerable<ResourceUsageRecord> records, Guid batchId, CancellationToken cancellationToken)
        {
            try
            {
                var batchOperation = new TableBatchOperation();
                foreach (var record in records)
                {
                    // Fill in record with Tenant info
                    var tenant = await TenantCacheClient.GetTenantAsync(record.EngagementAccount);
                    if (tenant == null)
                    {
                        BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, nameof(this.PushToTargetAsync), OperationStates.FailedMatch, $"Unable to get tenant for account '{record.EngagementAccount}'");
                    }
                    else if (this.pushUsageForWhitelistedSubscriptionsOnly)
                    {
                        if (Guid.TryParse(tenant.SubscriptionId, out Guid subId) && this.whitelistedSubscriptionIds.Contains(subId))
                        {
                            batchOperation.InsertOrReplace(new UsageRecord(record, tenant, batchId));
                        }
                        else
                        {
                            BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.PushToTargetAsync), OperationStates.Dropped, $"Drop usage for subscription '{tenant.SubscriptionId}' as it's not in whitelist");
                        }
                    }
                    else
                    {
                        batchOperation.InsertOrReplace(new UsageRecord(record, tenant, batchId));
                    }
                }

                if (batchOperation.Count > 0)
                {
                    var results = await this.table.ExecuteBatchAsync(batchOperation, cancellationToken);
                    BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.PushToTargetAsync), OperationStates.Succeeded, $"Push usage with PartitionId = {batchId}, count = {records.Count()}");
                }
            }
            catch (Exception ex)
            {
                BillingEventSource.Current.ErrorException(BillingEventSource.EmptyTrackingId, this, nameof(this.PushToTargetAsync), OperationStates.Failed, string.Empty, ex);
                throw ex;
            }
        }

        private async Task LoadBucketsAsync(CancellationToken cancellationToken)
        {
            var bucketStates = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, BillingUsageBucketState>>(BucketStatusKeyName);
            using (var tx = this.stateManager.CreateTransaction())
            {
                for (var i = 0; i < MaxBucketCount; i++)
                {
                    var bucketKey = string.Format(BucketKeyTemplate, i);
                    var state = await bucketStates.GetOrAddAsync(tx, bucketKey, BillingUsageBucketState.Idle, StateOperationTimeout, cancellationToken);
                    this.buckets.Add(bucketKey, new BillingUsageBucket(this.stateManager, bucketKey, state));
                }

                await tx.CommitAsync();
            }

            // Ensure at least one bucket open
            var bucket = await GetOpenBucketAsync(cancellationToken);
            BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.LoadBucketsAsync), OperationStates.Succeeded, $"Bucket {bucket.BucketKey} is opened from loading");

            // Re-send any bucket in sending/fault state
            var states = new List<BillingUsageBucketState>
            {
                BillingUsageBucketState.Sending,
                BillingUsageBucketState.Fault
            };
            bucket = await GetNextBucketInStateAsync(null, false, states, cancellationToken);
            while (bucket != null)
            {
                var currentState = bucket.State;
                if (await PushBucketAsync(bucket, cancellationToken))
                {
                    await UpdateBucketStateAsync(bucket.BucketKey, currentState, BillingUsageBucketState.Idle, cancellationToken);
                }
                else
                {
                    await UpdateBucketStateAsync(bucket.BucketKey, currentState, BillingUsageBucketState.Fault, cancellationToken);
                }

                bucket = await GetNextBucketInStateAsync(null, false, states, cancellationToken);
            }
        }

        private Task<BillingUsageBucket> GetNextBucketInStateAsync(string fromBucket, bool exclude, List<BillingUsageBucketState> states, CancellationToken cancellationToken)
        {
            // If fromBucket is null, search from begining
            var skip = !string.IsNullOrEmpty(fromBucket);

            BillingUsageBucket bucket = null;

            this.bucketStateLock.EnterReadLock();
            try
            {
                // Loop to 2 * count of bucket size, to start from fromBucket
                for (var i = 0; i < this.buckets.Count * 2; i++)
                {
                    var current = this.buckets.Values.ElementAt(i % this.buckets.Count);
                    if (skip && current.BucketKey == fromBucket)
                    {
                        skip = false;
                        if (exclude)
                        {
                            continue;
                        }
                    }

                    if (skip)
                    {
                        continue;
                    }

                    if (states.Contains(current.State))
                    {
                        bucket = current;
                        break;
                    }
                }
            }
            finally
            {
                this.bucketStateLock.ExitReadLock();
            }

            return Task.FromResult(bucket);
        }

        private async Task<BillingUsageBucket> GetOpenBucketAsync(CancellationToken cancellationToken)
        {
            var bucket = await GetNextBucketInStateAsync(null, false, new List<BillingUsageBucketState> { BillingUsageBucketState.Open }, cancellationToken);
            if (bucket != null)
            {
                return bucket;
            }

            return await OpenNextBucketAsync(null, false, cancellationToken);
        }

        private async Task<BillingUsageBucket> OpenNextBucketAsync(string currentBucket, bool exclude, CancellationToken cancellationToken)
        {
            // Find next bucket in idle/open/fault state
            // If that bucket is already opened (in case of App restart during switch bucket), no need to update status
            // If that bucket is in fault (in case push bucket failed), re-open to accept data and later push again
            var states = new List<BillingUsageBucketState>
            {
                BillingUsageBucketState.Idle,
                BillingUsageBucketState.Open,
                BillingUsageBucketState.Fault
            };

            ApplicationException exception;
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(1));

            while (!cancellationToken.IsCancellationRequested && !cts.Token.IsCancellationRequested)
            {
                var bucket = await this.GetNextBucketInStateAsync(currentBucket, exclude, states, cancellationToken);
                if (bucket == null)
                {
                    exception = new ApplicationException("No bucket to be opened");
                    BillingEventSource.Current.ErrorException(BillingEventSource.EmptyTrackingId, this, nameof(this.OpenNextBucketAsync), OperationStates.Failed, string.Empty, exception);
                    throw exception;
                }

                try
                {
                    var currentState = bucket.State;
                    if (currentState == BillingUsageBucketState.Open || await UpdateBucketStateAsync(bucket.BucketKey, currentState, BillingUsageBucketState.Open, cancellationToken))
                    {
                        BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.OpenNextBucketAsync), OperationStates.Succeeded, $"Bucket {bucket.BucketKey} is opened");
                        return bucket;
                    }
                }
                catch (Exception ex)
                {
                    BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, nameof(this.OpenNextBucketAsync), OperationStates.Failed, ex.ToString());
                }
            }

            exception = new ApplicationException(cts.Token.IsCancellationRequested ? "No bucket opened after 1min" : "Operation is cancelled");
            BillingEventSource.Current.ErrorException(BillingEventSource.EmptyTrackingId, this, nameof(this.OpenNextBucketAsync), OperationStates.Failed, string.Empty, exception);
            throw exception;
        }

        private async Task<bool> UpdateBucketStateAsync(string bucketKey, BillingUsageBucketState from, BillingUsageBucketState to, CancellationToken cancellationToken)
        {
            if (to == from)
            {
                return true;
            }

            this.bucketStateLock.EnterWriteLock();

            bool success = false;
            try
            {
                var bucket = this.buckets[bucketKey];
                if (bucket.State == from)
                {
                    BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.UpdateBucketStateAsync), OperationStates.Succeeded, $"Updated bucket '{bucketKey}' from {from.ToString()} to {to.ToString()}");
                    bucket.State = to;
                    success = true;
                }
                else
                {
                    BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, nameof(this.UpdateBucketStateAsync), OperationStates.Failed, $"Failed to update bucket '{bucketKey}' from {from.ToString()} to {to.ToString()}. Current state is {bucket.State.ToString()}");
                }
            }
            catch (Exception ex)
            {
                BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, nameof(this.UpdateBucketStateAsync), OperationStates.Failed, $"Failed to update bucket '{bucketKey}' from {from.ToString()} to {to.ToString()}. ex={ex.ToString()}");
            }
            finally
            {
                this.bucketStateLock.ExitWriteLock();
            }

            // Save new state to reliable store
            if (success)
            {
                try
                {
                    var bucketStates = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, BillingUsageBucketState>>(BucketStatusKeyName);
                    using (var tx = this.stateManager.CreateTransaction())
                    {
                        if (!await bucketStates.TryUpdateAsync(tx, bucketKey, to, from, StateOperationTimeout, cancellationToken))
                        {
                            BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, nameof(this.UpdateBucketStateAsync), OperationStates.Failed, $"Failed to update bucket to reliable '{bucketKey}' from {from.ToString()} to {to.ToString()}");
                        }

                        await tx.CommitAsync();
                    }
                }
                catch (Exception ex)
                {
                    BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, nameof(this.UpdateBucketStateAsync), OperationStates.Failed, $"Failed to update bucket to reliable '{bucketKey}' from {from.ToString()} to {to.ToString()}. ex={ex.ToString()}");
                }
            }

            return success;
        }

        private async Task<bool> PushBucketAsync(BillingUsageBucket bucket, CancellationToken cancellationToken)
        {
            try
            {
                var records = await bucket.GetRecordsAsync(cancellationToken);
                if (records == null || records.Count <= 0)
                {
                    BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, nameof(this.PushBucketAsync), OperationStates.Dropped, $"Bucket '{bucket.BucketKey}' has no records.");
                    return true;
                }

                var batchId = Guid.NewGuid();
                var pushAny = 0;

                // Push to table
                Func<IEnumerable<ResourceUsageRecord>, Guid, Task> pushBatchFunc = async (batch, id) =>
                {
                    bool succeed = await this.PushUsageBatchAsync(batch, id, cancellationToken);
                    if (succeed)
                    {
                        Interlocked.Exchange(ref pushAny, 1);
                    }
                };

                await this.ParallelPushAsync(records, batchId, pushBatchFunc);

                if (pushAny > 0)
                {
                    // Notify to queue
                    await NotifyPushAsync(batchId, cancellationToken);
                    await bucket.ClearAsync();
                }

                return pushAny > 0;
            }
            catch (Exception ex)
            {
                BillingEventSource.Current.ErrorException(BillingEventSource.EmptyTrackingId, this, nameof(this.PushBucketAsync), OperationStates.Failed, string.Empty, ex);
                return false;
            }
        }

        private async Task NotifyPushAsync(Guid batchId, CancellationToken cancellationToken)
        {
            var usageNotification = new UsageNotification();
            usageNotification.PartitionId = batchId.ToString();
            usageNotification.BatchId = Guid.NewGuid();

            var notificationContent = JsonConvert.SerializeObject(usageNotification, Formatting.None);
            CloudQueueMessage queueMessage = new CloudQueueMessage(notificationContent);
            var retry = 3;

            while (retry-- > 0)
            {
                try
                {
                    await this.queue.AddMessageAsync(queueMessage, cancellationToken);
                    BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.PushBucketAsync), OperationStates.Succeeded, $"Notify usage with PartitionId = {usageNotification.PartitionId}, BatchId = {usageNotification.BatchId}");
                    return;
                }
                catch (Exception ex)
                {
                    BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, nameof(this.PushBucketAsync), OperationStates.FailedNotFaulting, ex.ToString());
                    await Task.Delay(5 * 1000);
                }
            }

            throw new ApplicationException($"Failed to notify for batch {batchId}");
        }

        private void PrintBucketState()
        {
            if (this.buckets == null)
            {
                return;
            }

            var stateLog = string.Empty;
            foreach (var bucket in this.buckets)
            {
                stateLog += $"[{bucket.Key}:{bucket.Value.State.ToString()}]";
            }

            BillingEventSource.Current.Info(BillingEventSource.EmptyTrackingId, this, nameof(this.PrintBucketState), OperationStates.Empty, stateLog);
        }
    }
}
