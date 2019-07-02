// <copyright file="Signature.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class Signature
    {
        [JsonProperty(PropertyName = "Signature", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "Type", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ChannelType ChannelType { get; set; }

        [JsonProperty(PropertyName = "State", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceState State { get; set; }

        [JsonProperty(PropertyName = "StateMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonIgnore]
        public string EngagementAccount { get; set; }

        [JsonIgnore]
        public virtual string ExtendedCode { get; set; }
    }
}
