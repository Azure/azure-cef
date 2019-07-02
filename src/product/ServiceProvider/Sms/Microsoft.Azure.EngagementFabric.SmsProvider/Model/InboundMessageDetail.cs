// <copyright file="InboundMessageDetail.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class InboundMessageDetail
    {
        [JsonProperty(PropertyName = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "Extend")]
        public string ExtendedCode { get; set; }

        [JsonProperty(PropertyName = "Signature")]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "Message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "ReplyTime")]
        public DateTime? InboundTime { get; set; }
    }
}
