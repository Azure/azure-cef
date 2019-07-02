// <copyright file="AccountsCreateOrUpdateExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class AccountsCreateOrUpdateExample : EngagementFabricExampleBase
    {
        public AccountsCreateOrUpdateExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
            this.ResourceGroupName = DefaultResourceGroupName;
            this.AccountName = DefaultAccountName;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;
            this.Parameters = new
            {
                account = new Account
                {
                    Location = DefaultAccount.Location,
                    SKU = new SKU
                    {
                        Name = DefaultAccount.SKU.Name
                    }
                }
            };

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Body = DefaultAccount
            };
        }
    }
}
