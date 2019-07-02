// <copyright file="Account.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class Account
    {
        public Account()
        {
            this.AccountSettings = new AccountSettings();
        }

        public Account(string account)
            : this()
        {
            this.EngagementAccount = account;
        }

        [JsonProperty(PropertyName = "Account", Required = Required.Always)]
        public string EngagementAccount { get; set; }

        [JsonProperty(PropertyName = "AccountSettings")]
        public AccountSettings AccountSettings { get; set; }

        [JsonProperty(PropertyName = "SubscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty(PropertyName = "Provider")]
        public string Provider { get; set; }
    }
}
