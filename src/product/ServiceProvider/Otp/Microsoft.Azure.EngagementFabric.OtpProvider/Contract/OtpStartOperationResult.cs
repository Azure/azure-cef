// -----------------------------------------------------------------------
// <copyright file="OtpStartOperationResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Contract
{
    public class OtpStartOperationResult
    {
        public OtpStartOperationResult()
        {
        }

        [JsonProperty(PropertyName = "expireTime")]
        public int ExpireTime { get; set; }
    }
}
