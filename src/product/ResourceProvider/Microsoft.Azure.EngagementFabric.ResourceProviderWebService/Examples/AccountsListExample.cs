// <copyright file="AccountsListExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class AccountsListExample : EngagementFabricExampleBase
    {
        public AccountsListExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
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
