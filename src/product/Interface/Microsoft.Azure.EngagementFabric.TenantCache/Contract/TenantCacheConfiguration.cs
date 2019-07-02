// <copyright file="TenantCacheConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public class TenantCacheConfiguration
    {
        [DataMember]
        public string ConnectionString { get; set; }

        [DataMember]
        public int DatabaseId { get; set; }
    }
}
