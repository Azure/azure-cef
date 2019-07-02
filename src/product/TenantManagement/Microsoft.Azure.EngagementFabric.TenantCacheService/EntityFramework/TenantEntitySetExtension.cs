// <copyright file="TenantEntitySetExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.EntityFramework
{
    internal static class TenantEntitySetExtension
    {
        public static async Task<TenantEntity> GetAsync(
            this DbSet<TenantEntity> tenants,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            bool throwException = true)
        {
            var entity = await tenants.SingleOrDefaultAsync(e =>
                e.AccountName == accountName
                && e.SubscriptionId == subscriptionId
                && e.ResourceGroup == resourceGroupName);

            if (entity == null && throwException)
            {
                throw new ResourceNotFoundException($"Can not find account '{accountName}' in subscription '{subscriptionId}', resource group '{resourceGroupName}'");
            }

            return entity;
        }
    }
}
