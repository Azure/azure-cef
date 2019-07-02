// <copyright file="Domain.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class Domain
    {
        [JsonProperty(PropertyName = "DomainName", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "State", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ResourceState State { get; set; }

        [JsonProperty(PropertyName = "StateMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonIgnore]
        public string EngagementAccount { get; set; }
    }
}
