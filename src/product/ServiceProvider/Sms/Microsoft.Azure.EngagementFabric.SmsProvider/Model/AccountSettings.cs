// <copyright file="AccountSettings.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class AccountSettings
    {
        public AccountSettings()
        {
            PromotionRestricted = true;
        }

        [JsonProperty(PropertyName = "PromotionRestricted")]
        public bool PromotionRestricted { get; set; }
    }
}
