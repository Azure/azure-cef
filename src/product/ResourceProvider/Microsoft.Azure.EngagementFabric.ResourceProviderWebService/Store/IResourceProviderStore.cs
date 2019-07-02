// <copyright file="IResourceProviderStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.EntityFramework;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Store
{
    internal interface IResourceProviderStore
    {
        Task<SubscriptionRegistration> GetSubscriptionRegistrationAsync(
            string subscriptionId);

        Task<bool> IsSubscriptionRegisteredAsync(
            string subscriptionId);

        Task SetSubscriptionRegistrationAsync(
            string subscriptionId,
            SubscriptionRegistration subscriptionRegistration);
    }
}
