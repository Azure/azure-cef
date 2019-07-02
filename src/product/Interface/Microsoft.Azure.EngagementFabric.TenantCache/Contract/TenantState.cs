// <copyright file="TenantState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public enum TenantState
    {
        [EnumMember]
        Creating = 0,

        [EnumMember]
        Active = 1,

        [EnumMember]
        Updating = 2,

        [EnumMember]
        Deleting = 3,

        [EnumMember]
        Broken = 4,

        [EnumMember]
        Restoring = 5,

        [EnumMember]
        Failed = 6,  // internal state resulted if resource failed creation or deletion

        [EnumMember]
        Unknown = 7
    }
}
