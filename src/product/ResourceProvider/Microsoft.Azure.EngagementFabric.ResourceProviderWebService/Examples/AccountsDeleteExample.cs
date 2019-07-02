// <copyright file="AccountsDeleteExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class AccountsDeleteExample : EngagementFabricExampleBase
    {
        public AccountsDeleteExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
            this.ResourceGroupName = DefaultResourceGroupName;
            this.AccountName = DefaultAccountName;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
