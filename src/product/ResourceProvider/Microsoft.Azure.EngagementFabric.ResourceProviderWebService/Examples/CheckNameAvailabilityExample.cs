// <copyright file="CheckNameAvailabilityExample.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class CheckNameAvailabilityExample : EngagementFabricExampleBase
    {
        public CheckNameAvailabilityExample()
        {
            this.SubscriptionId = DefaultSubscriptionId;
            this.ApiVersion = ApiVersionStore.DefaultApiVersion;
            this.Parameters = new CheckNameAvailabilityParameter
            {
                Name = DefaultAccountName,
                Type = NameStore.FullyQualifiedAccountResourceType
            };

            this.Response = new ResponseModel
            {
                StatusCode = HttpStatusCode.OK,
                Body = new CheckNameAvailabilityResult
                {
                    NameAvailabile = false,
                    Reason = CheckNameUnavailableReason.AlreadyExists,
                    Message = $"Account '{DefaultAccountName}' already exists"
                }
            };
        }
    }
}
