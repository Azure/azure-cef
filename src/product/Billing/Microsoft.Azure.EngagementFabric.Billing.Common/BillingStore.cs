// <copyright file="BillingStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.Billing.Common
{
    public abstract class BillingStore
    {
        private readonly TimeSpan usagePushInterval;
        private readonly TimeSpan pushRetryInterval;
        private readonly TimeSpan pushRetryDuration;
        private readonly int batchSize;
        private readonly int parallelTaskCount;

        public BillingStore(
            TimeSpan usagePushInterval,
            TimeSpan pushRetryInterval,
            TimeSpan pushRetryDuration,
            int batchSize,
            int parallelTaskCount)
        {
            this.usagePushInterval = usagePushInterval;
            this.pushRetryInterval = pushRetryInterval;
            this.pushRetryDuration = pushRetryDuration;
            this.batchSize = batchSize;
            this.parallelTaskCount = parallelTaskCount;
        }

        public virtual async Task OnRunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(this.usagePushInterval, cancellationToken);
                await PushUsageInnerLoopAsync(cancellationToken);
            }
        }

        public virtual Task OnCloseAsync()
        {
            return Task.CompletedTask;
        }

        public abstract Task StoreBillingUsageAsync(IEnumerable<ResourceUsageRecord> records, CancellationToken cancellationToken);

        protected abstract Task PushUsageInnerLoopAsync(CancellationToken cancellationToken);

        protected abstract Task PushToTargetAsync(IEnumerable<ResourceUsageRecord> records, Guid batchId, CancellationToken cancellationToken);

        protected async Task ParallelPushAsync(IEnumerable<ResourceUsageRecord> enumerable, Guid batchId, Func<IEnumerable<ResourceUsageRecord>, Guid, Task> pushTask)
        {
            List<Task> tasks = new List<Task>();
            int remainingCount = enumerable.Count();
            IEnumerable<ResourceUsageRecord> remainingEntities = enumerable;
            while (remainingCount > 0)
            {
                IEnumerable<ResourceUsageRecord> batch;
                int currentBatchSize;
                if (remainingCount > batchSize)
                {
                    currentBatchSize = batchSize;
                    batch = remainingEntities.Take(currentBatchSize);
                    remainingEntities = remainingEntities.Skip(currentBatchSize);
                }
                else
                {
                    currentBatchSize = remainingCount;
                    batch = remainingEntities;
                }

                tasks.Add(pushTask(batch, batchId));
                if (tasks.Count == parallelTaskCount)
                {
                    await Task.WhenAll(tasks);
                    tasks.Clear();
                }

                remainingCount -= currentBatchSize;
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }

        protected async Task<bool> PushUsageBatchAsync(IEnumerable<ResourceUsageRecord> records, Guid batchId, CancellationToken cancellationToken)
        {
            var attemptNumber = 1;
            var succeed = false;
            var startTime = DateTime.UtcNow;
            var waitBeforePush = false;

            do
            {
                try
                {
                    if (waitBeforePush)
                    {
                        attemptNumber++;
                        await Task.Delay(pushRetryInterval, cancellationToken);
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }

                    await PushToTargetAsync(records, batchId, cancellationToken);
                    succeed = true;
                }
                catch
                {
                    waitBeforePush = true;
                }
            }
            while (!succeed && ((DateTime.UtcNow - startTime) <= pushRetryDuration));

            return succeed;
        }
    }
}
