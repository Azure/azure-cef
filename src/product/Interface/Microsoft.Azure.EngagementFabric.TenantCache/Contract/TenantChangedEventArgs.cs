// <copyright file="TenantChangedEventArgs.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public class TenantChangedEventArgs
    {
        [DataMember]
        public TenantChangedEventType EventType { get; set; }

        [DataMember]
        public string TenantName { get; set; }

        [DataMember]
        public Tenant UpdatedTenant { get; set; }
    }
}
