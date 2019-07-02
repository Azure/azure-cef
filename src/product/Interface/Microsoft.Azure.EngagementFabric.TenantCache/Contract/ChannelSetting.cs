// <copyright file="ChannelSetting.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public class ChannelSetting
    {
        [DataMember(Name = "Name", Order = 1, IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "Type", Order = 2, IsRequired = true)]
        public string Type { get; set; }

        [DataMember(Name = "Functions", Order = 3)]
        public IEnumerable<string> Functions { get; set; }

        [DataMember(Name = "Credentials", Order = 4)]
        public Dictionary<string, string> Credentials { get; set; }
    }
}