// -----------------------------------------------------------------------
// <copyright file="AuthenticationRule.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public class AuthenticationRule : IExtensibleDataObject
    {
        [DataMember(Name = "KeyName", Order = 1)]
        public string KeyName { get; set; }

        [DataMember(Name = "PrimaryKey", Order = 2)]
        public string PrimaryKey { get; set; }

        [DataMember(Name = "SecondaryKey", Order = 3)]
        public string SecondaryKey { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
