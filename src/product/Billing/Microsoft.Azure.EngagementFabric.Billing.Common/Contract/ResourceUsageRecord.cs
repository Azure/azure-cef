// <copyright file="ResourceUsageRecord.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Billing.Common.Contract
{
    [DataContract]
    public class ResourceUsageRecord : IExtensibleDataObject
    {
        public ResourceUsageRecord()
        {
        }

        public ResourceUsageRecord(ResourceUsageRecord other)
        {
            this.EngagementAccount = other.EngagementAccount;
            this.UsageType = other.UsageType;
            this.Quantity = other.Quantity;
        }

        [DataMember]
        public string EngagementAccount { get; set; }

        [DataMember]
        public ResourceUsageType UsageType { get; set; }

        [DataMember]
        public long Quantity { get; set; }

        ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }
    }
}
