// <copyright file="GroupCreateOrUpdateResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class GroupCreateOrUpdateResult
    {
        [DataMember]
        public Group Group { get; set; }

        [DataMember]
        public List<GroupCreateOrUpdateResultEntry> ErrorList { get; set; }

        [DataContract]
        public class GroupCreateOrUpdateResultEntry
        {
            [DataMember]
            public string Email { get; set; }

            [DataMember]
            public string ErrorMessage { get; set; }
        }
    }
}
