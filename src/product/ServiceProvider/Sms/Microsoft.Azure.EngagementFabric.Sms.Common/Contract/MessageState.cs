// <copyright file="MessageState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    // Message state code in reporting detail returned
    [DataContract]
    public enum MessageState
    {
        [EnumMember]
        UNKNOWN,

        [EnumMember]
        DELIVERED,

        [EnumMember]
        TIMEOUT,

        [EnumMember]
        FAILED_UNKNOWN,

        [EnumMember]
        FAILED_OPERATOR,

        [EnumMember]
        FAILED_MOBILE,

        [EnumMember]
        FAILED_MOBILE_REPEAT,

        [EnumMember]
        FAILED_BLACK_LIST,

        [EnumMember]
        FAILED_UNSUBSCRIBE,

        [EnumMember]
        FAILED_INTERCEPT,

        [EnumMember]
        FAILED_SIGN,

        [EnumMember]
        FAILED_SEND_FILTER
    }
}
