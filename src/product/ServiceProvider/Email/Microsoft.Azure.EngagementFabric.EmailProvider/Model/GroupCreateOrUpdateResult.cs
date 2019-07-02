// <copyright file="GroupCreateOrUpdateResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class GroupCreateOrUpdateResult
    {
        [JsonProperty(PropertyName = "GroupName", Required = Required.Always)]
        public string Value { get; set; }

        [JsonProperty(PropertyName = "Description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "State", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public GroupResultState State { get; set; }

        [JsonProperty(PropertyName = "Invalid", NullValueHandling = NullValueHandling.Ignore)]
        public List<GroupCreateOrUpdateResultEntry> Invalid { get; set; }

        public class GroupCreateOrUpdateResultEntry
        {
            [JsonProperty(PropertyName = "Email")]
            public string Email { get; set; }

            [JsonProperty(PropertyName = "ErrorMessage")]
            public string ErrorMessage { get; set; }
        }
    }
}
