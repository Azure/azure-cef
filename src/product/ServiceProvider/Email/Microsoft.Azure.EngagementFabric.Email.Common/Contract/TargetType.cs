// <copyright file="TargetType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public enum TargetType
    {
        [EnumMember]
        Invalid = 0,

        [EnumMember]
        List = 1,

        [EnumMember]
        Group = 2
    }
}
