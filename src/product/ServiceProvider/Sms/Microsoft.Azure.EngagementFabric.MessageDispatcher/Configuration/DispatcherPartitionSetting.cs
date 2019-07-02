// <copyright file="DispatcherPartitionSetting.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration
{
    public class DispatcherPartitionSetting
    {
        public int MessagePumpBatchSize { get; set; }

        public int MaxPushQueueLength { get; set; }

        public int MaximumDeliveryCount { get; set; }

        public ServiceConfigureSetting ServiceConfigureSetting { get; set; }

        public DispatcherQueueSetting InstantQueueSetting { get; set; }

        public List<DispatcherQueueSetting> DelayedQueueSettings { get; set; }

        public static DispatcherPartitionSetting Create()
        {
            var partitionSetting = new DispatcherPartitionSetting();
            partitionSetting.MessagePumpBatchSize = 1;
            partitionSetting.MaxPushQueueLength = 1000000;
            partitionSetting.MaximumDeliveryCount = 3;

            partitionSetting.ServiceConfigureSetting = new ServiceConfigureSetting();

            // Instant Queue
            partitionSetting.InstantQueueSetting = new DispatcherQueueSetting("InstantQueue", partitionSetting)
            {
                DeliveryType = DeliveryType.Instant,
                MaxQueueLength = 100000,
                PushWorkerCount = 1,
                MaximumPumpRetries = 3,
                RetryDelay = TimeSpan.Zero
            };

            // Delayed Queue
            partitionSetting.DelayedQueueSettings = new List<DispatcherQueueSetting>
            {
                new DispatcherQueueSetting("DelayedQueue_0_10s", partitionSetting)
                {
                    DeliveryType = DeliveryType.Delayed,
                    MaxQueueLength = 10000,
                    PushWorkerCount = 1,
                    MaximumPumpRetries = 3,
                    RetryDelay = TimeSpan.FromSeconds(10)
                }
            };

            return partitionSetting;
        }
    }
}
