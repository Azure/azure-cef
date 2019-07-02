// <copyright file="BillingUsageBucketState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.BillingService.Manager
{
    [DataContract]
    public enum BillingUsageBucketState
    {
        [EnumMember]
        Idle,

        [EnumMember]
        Open,

        [EnumMember]
        Sending,

        [EnumMember]
        Fault
    }
}
