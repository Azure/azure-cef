// <copyright file="ISubscriptionManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers
{
    /// <summary>
    /// The interface for subscription operations
    /// </summary>
    public interface ISubscriptionManager
    {
        /// <summary>
        /// Create or update subscription
        /// </summary>
        /// <param name="requestId">The request ID</param>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="model">Subscription state</param>
        /// <returns>List of impacted accounts</returns>
        Task<IEnumerable<string>> CreateOrUpdateSubscriptionAsync(
            string requestId,
            string subscriptionId,
            SubscriptionDescription model);
    }
}
