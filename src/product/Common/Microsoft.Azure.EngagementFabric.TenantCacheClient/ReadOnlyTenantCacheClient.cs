// <copyright file="ReadOnlyTenantCacheClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;

namespace Microsoft.Azure.EngagementFabric.TenantCache
{
    public class ReadOnlyTenantCacheClient : TenantCacheClientBase
    {
        private static ReadOnlyTenantCacheClient client;

        protected ReadOnlyTenantCacheClient(bool enableInMemoryCache)
            : base(enableInMemoryCache)
        {
        }

        public static ReadOnlyTenantCacheClient GetClient(bool enableInMemoryCache)
        {
            if (client == null)
            {
                client = new ReadOnlyTenantCacheClient(enableInMemoryCache);
            }

            return client;
        }

        public async Task<Tenant> GetTenantAsync(string engagementAccount)
        {
            Validator.ArgumentNotNullOrEmpty(engagementAccount, nameof(engagementAccount));

            Tenant tenant;

            // Get from memory
            if (this.EnableInMemoryCache)
            {
                if (this.MemoryCache.TryGetValue(engagementAccount, out tenant))
                {
                    return tenant;
                }
            }

            // Get from redis
            tenant = await this.RedisClient.GetAsync<Tenant>(engagementAccount);
            if (tenant != null)
            {
                if (this.EnableInMemoryCache)
                {
                    this.MemoryCache.AddOrUpdate(engagementAccount, tenant, (key, old) => tenant);
                }

                return tenant;
            }

            // Get from TenantCacheService
            return await this.CacheProxy.GetTenantAsyncInternal(engagementAccount);
        }
    }
}