// <copyright file="AccountsRegenerateKeyExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Linq;
using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class AccountsRegenerateKeyExample : EngagementFabricExampleBase
    {
        public AccountsRegenerateKeyExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
            this.ResourceGroupName = DefaultResourceGroupName;
            this.AccountName = DefaultAccountName;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;
            this.Parameters = new RegenerateKeyParameter
            {
                Name = DefaultKeys.First().Name,
                Rank = DefaultKeys.First().Rank
            };

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Body = DefaultKeys.First()
            };
        }
    }
}
