// <copyright file="ChannelType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public enum ChannelType
    {
        [EnumMember]
        Invalid = 0,

        [EnumMember]
        Industry = 1,

        [EnumMember]
        Marketing = 2,

        [EnumMember]
        Both = 3
    }
}
