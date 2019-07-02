// <copyright file="Template.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class Template
    {
        [JsonProperty(PropertyName = "TemplateName", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Signature", Required = Required.Always)]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "TplType", Required = Required.Always)]
        public MessageCategory Category { get; set; }

        [JsonProperty(PropertyName = "Message", Required = Required.Always)]
        public string Body { get; set; }

        [JsonProperty(PropertyName = "State", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceState State { get; set; }

        [JsonProperty(PropertyName = "StateMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonIgnore]
        public string EngagementAccount { get; set; }
    }
}
