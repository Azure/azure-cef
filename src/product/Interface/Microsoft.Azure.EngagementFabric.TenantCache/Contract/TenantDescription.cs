// -----------------------------------------------------------------------
// <copyright file="TenantDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public class TenantDescription : IExtensibleDataObject
    {
        [DataMember(Name = "AuthenticationRules", Order = 1)]
        public List<AuthenticationRule> AuthenticationRules { get; set; }

        [DataMember(Name = "ChannelSettings", Order = 2)]
        public List<ChannelSetting> ChannelSettings { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
