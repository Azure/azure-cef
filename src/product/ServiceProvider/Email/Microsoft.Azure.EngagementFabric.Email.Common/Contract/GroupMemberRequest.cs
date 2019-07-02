// <copyright file="GroupMemberRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class GroupMemberRequest
    {
        [DataMember]
        public string ContinuationToken { get; set; }

        [DataMember]
        public int Count { get; set; }
    }
}
