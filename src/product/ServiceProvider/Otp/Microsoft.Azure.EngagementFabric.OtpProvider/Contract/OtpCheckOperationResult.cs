// -----------------------------------------------------------------------
// <copyright file="OtpCheckOperationResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Contract
{
    public class OtpCheckOperationResult
    {
        public OtpCheckOperationResult()
        {
        }

        public OtpCheckOperationResult(OtpOperationStatus status)
        {
            Status = status;
        }

        [JsonProperty(PropertyName = "State")]
        [JsonConverter(typeof(StringEnumConverter))]
        public OtpOperationStatus Status { get; set; }
    }
}
