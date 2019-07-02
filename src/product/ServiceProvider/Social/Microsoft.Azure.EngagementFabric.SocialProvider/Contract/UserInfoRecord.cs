// -----------------------------------------------------------------------
// <copyright file="UserInfoRecord.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Contract
{
    [DataContract]
    public class UserInfoRecord : IExtensibleDataObject
    {
        [DataMember(Name = "Description", Order = 1)]
        public UserInfoRecordDescription Description { get; set; }

        [DataMember(Name = "ExpirationTime", Order = 2, EmitDefaultValue = false)]
        public DateTime? ExpirationTime { get; set; }

        [DataMember(Name = "CreatedTime", Order = 3)]
        public DateTime CreatedTime { get; set; }

        [DataMember(Name = "ModifiedTime", Order = 4)]
        public DateTime ModifiedTime { get; set; }

        public ExtensionDataObject ExtensionData { get; set; }
    }
}
