// -----------------------------------------------------------------------
// <copyright file="AccountKey.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public class AccountKey
    {
        [DataMember(Name = "Name", Order = 1)]
        public string Name { get; set; }

        [DataMember(Name = "IsPrimaryKey", Order = 2)]
        public bool IsPrimaryKey { get; set; }

        [DataMember(Name = "Value", Order = 3)]
        public string Value { get; set; }
    }
}
