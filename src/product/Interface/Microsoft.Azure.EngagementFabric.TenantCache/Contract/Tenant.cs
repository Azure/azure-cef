// <copyright file="Tenant.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public class Tenant
    {
        [DataMember(Name = "SubscriptionId", Order = 1, IsRequired = true)]
        public string SubscriptionId { get; set; }

        [DataMember(Name = "ResourceGroupName", Order = 2, IsRequired = true)]
        public string ResourceGroupName { get; set; }

        [DataMember(Name = "AccountName", Order = 3, IsRequired = true)]
        public string AccountName { get; set; }

        [DataMember(Name = "Location", Order = 4, IsRequired = true)]
        public string Location { get; set; }

        [DataMember(Name = "SKU", Order = 5, IsRequired = true)]
        public string SKU { get; set; }

        [DataMember(Name = "Tags", Order = 6, IsRequired = true)]
        public Dictionary<string, string> Tags { get; set; }

        [DataMember(Name = "State", Order = 7, IsRequired = true)]
        public TenantState State { get; set; }

        [DataMember(Name = "Address", Order = 8, IsRequired = false)]
        public string Address { get; set; }

        [DataMember(Name = "TenantDescription", Order = 9, IsRequired = true)]
        public TenantDescription TenantDescription { get; set; }

        [DataMember(Name = "ResourceId", Order = 10, IsRequired = false)]
        public string ResourceId { get; set; }

        [DataMember(Name = "IsDisabled", Order = 11, IsRequired = false)]
        public bool? IsDisabled { get; set; }

        public bool IsInScope(string subscriptionId, string resourceGroupName)
        {
            return string.Equals(this.SubscriptionId, subscriptionId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(this.ResourceGroupName, resourceGroupName, StringComparison.OrdinalIgnoreCase);
        }
    }
}