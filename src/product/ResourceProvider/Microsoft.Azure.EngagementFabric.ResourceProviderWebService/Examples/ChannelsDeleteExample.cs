// <copyright file="ChannelsDeleteExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class ChannelsDeleteExample : EngagementFabricExampleBase
    {
        public ChannelsDeleteExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
            this.ResourceGroupName = DefaultResourceGroupName;
            this.AccountName = DefaultAccountName;
            this.ChannelName = DefaultChannelName;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
