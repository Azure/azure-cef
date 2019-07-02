// <copyright file="InboundType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public enum InboundType
    {
        [EnumMember]
        Unknown,

        [EnumMember]
        Report,

        [EnumMember]
        MoMessage
    }
}
