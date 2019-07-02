// <copyright file="Account.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.Common.Collection;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class Account
    {
        public Account(string account)
        {
            this.EngagementAccount = account;
            this.Properties = new PropertyCollection<string>();
        }

        [JsonProperty(PropertyName = "Account", Required = Required.Always)]
        public string EngagementAccount { get; set; }

        [JsonProperty(PropertyName = "SubscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty(PropertyName = "Properties")]
        public PropertyCollection<string> Properties { get; set; }
    }
}
