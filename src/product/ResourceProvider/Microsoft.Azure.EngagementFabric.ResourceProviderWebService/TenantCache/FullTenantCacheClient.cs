// <copyright file="FullTenantCacheClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.TenantCache
{
    internal class FullTenantCacheClient : TenantCacheClientBase
    {
        private static FullTenantCacheClient client;

        protected FullTenantCacheClient(bool enableInMemoryCache)
            : base(enableInMemoryCache)
        {
        }

        public static FullTenantCacheClient GetClient(bool enableInMemoryCache)
        {
            if (client == null)
            {
                client = new FullTenantCacheClient(enableInMemoryCache);
            }

            return client;
        }

        public async Task<Tenant> CreateOrUpdateTenantAsync(
            string requestId,
            Tenant tenant,
            AuthenticationRule[] authenticationRules,
            Dictionary<string, int> quotas)
        {
            return await this.CacheProxy.CreateOrUpdateTenantAsync(
                requestId,
                tenant,
                authenticationRules,
                quotas);
        }

        public async Task<Tenant> UpdateTenantAsync(
            string requestId,
            Tenant tenant)
        {
            return await this.CacheProxy.UpdateTenantAsync(
                requestId,
                tenant);
        }

        public async Task<bool> DeleteTenantAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            try
            {
                await this.CacheProxy.DeleteTenantAsync(
                    requestId,
                    subscriptionId,
                    resourceGroupName,
                    accountName);

                return true;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ResourceNotFoundException)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<Tenant> GetTenantAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            Tenant tenant;
            if (this.MemoryCache.TryGetValue(accountName, out tenant) && tenant.IsInScope(subscriptionId, resourceGroupName))
            {
                return tenant;
            }

            tenant = await this.RedisClient.GetAsync<Tenant>(accountName);
            if (tenant != null && tenant.IsInScope(subscriptionId, resourceGroupName))
            {
                if (this.EnableInMemoryCache)
                {
                    this.MemoryCache.AddOrUpdate(accountName, tenant, (key, old) => tenant);
                }

                return tenant;
            }

            return await this.CacheProxy.GetTenantAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName);
        }

        public async Task<IEnumerable<Tenant>> ListTenantsAsync(
            string requestId,
            string subscriptionId)
        {
            return await this.CacheProxy.ListTenantsAsync(
                requestId,
                subscriptionId);
        }

        public async Task<IEnumerable<Tenant>> ListTenantsAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName)
        {
            return await this.CacheProxy.ListTenantsByResourceGroupAsync(
                requestId,
                subscriptionId,
                resourceGroupName);
        }

        public async Task<AccountKey> ResetKeyAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            AccountKey accountKey)
        {
            return await this.CacheProxy.ResetKeyAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName,
                accountKey);
        }

        public async Task<ChannelSetting> CreateOrUpdateChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            string channelType,
            string[] channelFunctions,
            Dictionary<string, string> credentials)
        {
            return await this.CacheProxy.CreateOrUpdateChannelAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName,
                channelName,
                channelType,
                channelFunctions,
                credentials);
        }

        public async Task<bool> DeleteChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName)
        {
            try
            {
                await this.CacheProxy.DeleteChannelAsync(
                    requestId,
                    subscriptionId,
                    resourceGroupName,
                    accountName,
                    channelName);

                return true;
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is ResourceNotFoundException)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
        }

        public async Task<bool> AccountExistsAsync(
            string requestId,
            string accountName)
        {
            return await this.CacheProxy.AccountExistsAsync(
                requestId,
                accountName);
        }
    }
}
