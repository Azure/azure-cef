// <copyright file="OtpCheckDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Contract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.EngagementFabric.Common.Collection;
    using Newtonsoft.Json;

    public class OtpCheckDescription
    {
        [JsonProperty(PropertyName = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "Code")]
        public string Code { get; set; }
    }
}
