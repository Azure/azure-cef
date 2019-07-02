// <copyright file="ChannelsCreateOrUpdateExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class ChannelsCreateOrUpdateExample : EngagementFabricExampleBase
    {
        public ChannelsCreateOrUpdateExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
            this.ResourceGroupName = DefaultResourceGroupName;
            this.AccountName = DefaultAccountName;
            this.ChannelName = DefaultChannelName;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;
            this.Parameters = new
            {
                channel = new Channel
                {
                    Properties = DefaultChannelProperties
                }
            };

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Body = DefaultResponseChannel
            };
        }
    }
}
