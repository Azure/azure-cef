// <copyright file="Template.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class Template
    {
        [JsonProperty(PropertyName = "TemplateName", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "SenderAddrID", Required = Required.Always)]
        public Guid SenderId { get; set; }

        [JsonProperty(PropertyName = "SenderAlias", Required = Required.Always)]
        public string SenderAlias { get; set; }

        [JsonProperty(PropertyName = "Subject", Required = Required.Always)]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "HtmlMsg", Required = Required.Always)]
        public string HtmlMsg { get; set; }

        [JsonProperty(PropertyName = "EnableUnsubscribe", Required = Required.AllowNull)]
        public bool? EnableUnSubscribe { get; set; }

        [JsonProperty(PropertyName = "State", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceState State { get; set; }

        [JsonProperty(PropertyName = "StateMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string StateMessage { get; set; }

        [JsonIgnore]
        public string EngagementAccount { get; set; }
    }
}
