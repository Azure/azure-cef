// <copyright file="AccountsListByResourceGroupExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class AccountsListByResourceGroupExample : EngagementFabricExampleBase
    {
        public AccountsListByResourceGroupExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
            this.ResourceGroupName = DefaultResourceGroupName;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Body = new AccountList
                {
                    Accounts = DefaultAccounts
                }
            };
        }
    }
}
