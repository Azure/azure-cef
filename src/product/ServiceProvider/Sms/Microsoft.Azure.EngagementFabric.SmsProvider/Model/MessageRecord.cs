// <copyright file="MessageRecord.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class MessageRecord
    {
        [JsonProperty(PropertyName = "MessageId", Required = Required.Always)]
        public string MessageId { get; set; }

        [JsonProperty(PropertyName = "SendTime", Required = Required.Always)]
        public DateTime? SendTime { get; set; }

        [JsonProperty(PropertyName = "Targets", Required = Required.Always)]
        public int Targets { get; set; }

        [JsonProperty(PropertyName = "Delivered", Required = Required.Always)]
        public int Succeed { get; set; }

        [JsonProperty(PropertyName = "Failed", Required = Required.Always)]
        public int Failed { get; set; }
    }
}
