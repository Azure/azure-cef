// <copyright file="MessageSendResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class MessageSendResponse
    {
        [JsonProperty(PropertyName = "MessageId")]
        public Guid MessageId { get; set; }

        [JsonProperty(PropertyName = "SendTime")]
        public DateTime SendTime { get; set; }
    }
}
