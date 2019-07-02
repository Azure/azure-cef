// <copyright file="RequestOutcome.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    /// <summary>
    /// Response code mapped from third party providers
    /// </summary>
    [DataContract]
    public enum RequestOutcome
    {
        // Shared
        [EnumMember]
        UNKNOWN = 0,

        [EnumMember]
        SUCCESS = 100,

        [EnumMember]
        DELIVERING = 101,

        [EnumMember]
        TIMEOUT = 201,

        [EnumMember]
        CANCELLED = 202,

        [EnumMember]
        FAILED_OPERATOR = 203,

        [EnumMember]
        FAILED_UNAUTHORIZED = 204,

        [EnumMember]
        FAILED_DATA_CONTRACT = 205,

        [EnumMember]
        FAILED_CONTENT = 206,

        [EnumMember]
        FAILED_BALANCE = 207,

        [EnumMember]
        FAILED_UNKNOWN = 208,

        // SMS related
        [EnumMember]
        FAILED_OVER_SPEED = 301,

        [EnumMember]
        FAILED_MOBILE = 302,

        [EnumMember]
        FAILED_SIGN = 303,

        [EnumMember]
        FAILED_EXTENDED_CODE = 304,
    }
}
