// -----------------------------------------------------------------------
// <copyright file="OtpOperationStatus.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Contract
{
    public enum OtpOperationStatus
    {
        [EnumMember(Value = "SUCCESS")]
        SUCCESS,

        [EnumMember(Value = "WRONG_CODE")]
        WRONG_CODE,

        [EnumMember(Value = "CODE_EXPIRED")]
        CODE_EXPIRED
    }
}
