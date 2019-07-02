// <copyright file="DispatcherQueueSetting.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration
{
    public class DispatcherQueueSetting
    {
        public DispatcherQueueSetting(string name, DispatcherPartitionSetting partitionSetting)
        {
            this.Name = name;
            this.PartitionSetting = partitionSetting;
        }

        public string Name { get; }

        public DispatcherPartitionSetting PartitionSetting { get; }

        public DeliveryType DeliveryType { get; set; }

        public long MaxQueueLength { get; set; }

        public int PushWorkerCount { get; set; }

        public int MaximumPumpRetries { get; set; }

        public TimeSpan RetryDelay { get; set; }
    }
}
