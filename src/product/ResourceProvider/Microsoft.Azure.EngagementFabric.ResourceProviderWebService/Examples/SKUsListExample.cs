// <copyright file="SKUsListExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class SKUsListExample : EngagementFabricExampleBase
    {
        public SKUsListExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Body = new SkuDescriptionList
                {
                    SKUs = SkuStore.Descriptions
                }
            };
        }
    }
}
