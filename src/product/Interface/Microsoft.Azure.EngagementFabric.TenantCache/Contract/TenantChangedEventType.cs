// <copyright file="TenantChangedEventType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public enum TenantChangedEventType
    {
        [EnumMember]
        TenantUpdated,

        [EnumMember]
        TenantDeleted
    }
}
