// <copyright file="MessageDetails.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class MessageDetails : MessageRecord
    {
        [JsonProperty(PropertyName = "Details", Required = Required.Always)]
        public List<MessageDetailEntry> Details { get; set; }

        [JsonIgnore]
        public TableContinuationToken ContinuationToken { get; set; }

        public class MessageDetailEntry
        {
            [JsonProperty(PropertyName = "PhoneNumber", Required = Required.Always)]
            public string PhoneNumber { get; set; }

            [JsonProperty(PropertyName = "State", Required = Required.Always)]
            public string State { get; set; }

            [JsonProperty(PropertyName = "SendTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? SubmitTime { get; set; }

            [JsonProperty(PropertyName = "ReceiveTime", NullValueHandling = NullValueHandling.Ignore)]
            public DateTime? ReceiveTime { get; set; }
        }
    }
}
