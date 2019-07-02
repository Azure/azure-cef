// <copyright file="GroupMembers.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class GroupMembers
    {
        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public List<string> Emails { get; set; }

        [DataMember]
        public string ContinuationToken { get; set; }
    }
}
