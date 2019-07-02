// <copyright file="UsageNotification.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.BillingService.Store
{
    public class UsageNotification
    {
        public string PartitionId { get; set; }

        public Guid BatchId { get; set; }
    }
}
