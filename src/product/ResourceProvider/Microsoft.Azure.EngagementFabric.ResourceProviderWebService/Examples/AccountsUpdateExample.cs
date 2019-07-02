// <copyright file="AccountsUpdateExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class AccountsUpdateExample : EngagementFabricExampleBase
    {
        public AccountsUpdateExample()
        {
            var tags = new Dictionary<string, string>
            {
                {
                    "tagName", "tagValue"
                }
            };

            this.SubscriptionId = DefaultSubscriptionId;
            this.ResourceGroupName = DefaultResourceGroupName;
            this.AccountName = DefaultAccountName;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;
            this.Parameters = new
            {
                accountPatch = new AccountPatch
                {
                    Tags = tags
                }
            };

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Body = new Account
                {
                    Id = DefaultAccount.Id,
                    Name = DefaultAccount.Name,
                    Type = DefaultAccount.Type,
                    Location = DefaultAccount.Location,
                    SKU = DefaultAccount.SKU,
                    Tags = tags
                }
            };
        }
    }
}
