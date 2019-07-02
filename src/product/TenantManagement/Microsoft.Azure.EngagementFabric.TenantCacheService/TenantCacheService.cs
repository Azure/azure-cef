// <copyright file="TenantCacheService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Cache;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.Azure.EngagementFabric.TenantCacheService.Configuration;
using Microsoft.Azure.EngagementFabric.TenantCacheService.EntityFramework;
using Microsoft.Azure.EngagementFabric.TenantCacheService.Quota;
using Microsoft.Azure.EngagementFabric.TenantCacheService.Store;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class TenantCacheService : StatelessService, ITenantCache
    {
        private const int MaxRetryForConcurrency = 10;

        private const string TenantsKeyName = "Tenants";
        private readonly TenantConfiguration configuration;
        private IAdminStore store;
        private RedisClient cache;
        private IQuotaManager quotaManager;

        public TenantCacheService(StatelessServiceContext context)
            : base(context)
        {
            this.configuration = new TenantConfiguration(this.Context.CodePackageActivationContext);
        }

        #region Service Provider Method
        public Task<TenantCacheConfiguration> GetCacheConfiguration()
        {
            var config = new TenantCacheConfiguration
            {
                ConnectionString = this.configuration.TenantCache_DefaultConnectionString,
                DatabaseId = this.configuration.TenantCache_DatabaseId
            };

            return Task.FromResult(config);
        }

        public async Task<Tenant> GetTenantAsyncInternal(string accountName)
        {
            var trackingId = Guid.NewGuid().ToString();
            this.LogActionBegin(
                trackingId,
                accountName);

            var stored = (await this.store.GetTenantAsync(accountName))?.ToTenant();
            if (stored != null)
            {
                await this.SetTenantAsync(trackingId, stored);
            }

            this.LogActionEnd(trackingId);
            return stored;
        }

        public async Task<QuotaOperationResult> AcquireQuotaAsync(
            string engagementAccount,
            string quotaName,
            int required)
        {
            return await this.quotaManager.AcquireQuotaAsync(
                engagementAccount,
                quotaName,
                required);
        }

        public async Task<QuotaOperationResult> ReleaseQuotaAsync(
            string engagementAccount,
            string quotaName,
            int released)
        {
            return await this.quotaManager.ReleaseQuotaAsync(
                engagementAccount,
                quotaName,
                released);
        }

        public async Task CreateOrUpdateQuotaAsync(
            string engagementAccount,
            string quotaName,
            int quota)
        {
            await this.quotaManager.CreateOrUpdateQuotaAsync(
                engagementAccount,
                quotaName,
                quota);
        }

        public async Task RemoveQuotaAsync(
            string engagementAccount,
            string quotaName)
        {
            await this.quotaManager.RemoveQuotaAsync(
                engagementAccount,
                quotaName);
        }

        #endregion

        #region Resource provider methods
        public async Task<Tenant> CreateOrUpdateTenantAsync(
            string requestId,
            Tenant tenant,
            AuthenticationRule[] authenticationRules,
            Dictionary<string, int> quotas)
        {
            this.LogActionBegin(
                requestId,
                tenant.AccountName,
                $"Quotas = {string.Join(", ", quotas.Select(pair => $"{pair.Key} = {pair.Value}"))}");

            var updated = await this.store.CreateOrUpdateTenantAsync(
                tenant,
                authenticationRules,
                quotas);

            await this.SetTenantAsync(requestId, updated);

            this.LogActionEnd(requestId);
            return updated;
        }

        public async Task<Tenant> UpdateTenantAsync(
            string requestId,
            Tenant tenant)
        {
            this.LogActionBegin(
                requestId,
                tenant.AccountName);

            var updated = await this.store.UpdateTenantAsync(
                tenant);

            await this.SetTenantAsync(requestId, updated);

            this.LogActionEnd(requestId);
            return updated;
        }

        public async Task DeleteTenantAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            this.LogActionBegin(
                requestId,
                accountName);

            await this.store.DeleteTenantAsync(
                subscriptionId,
                resourceGroupName,
                accountName);

            await this.DeleteTenantAsync(
                requestId,
                accountName);

            this.LogActionEnd(requestId);
        }

        public async Task<Tenant> GetTenantAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            this.LogActionBegin(
                requestId,
                accountName);

            var tenant = await this.store.GetTenantAsync(
                subscriptionId,
                resourceGroupName,
                accountName);

            await this.SetTenantAsync(requestId, tenant);

            this.LogActionEnd(requestId);
            return tenant;
        }

        public async Task<Tenant[]> ListTenantsAsync(
            string requestId,
            string subscriptionId)
        {
            this.LogActionBegin(
                requestId,
                string.Empty);

            var tenants = this.store.ListTenants(
                subscriptionId);

            foreach (var tenant in tenants)
            {
                await this.SetTenantAsync(requestId, tenant);
            }

            this.LogActionEnd(requestId);
            return tenants.ToArray();
        }

        public async Task<Tenant[]> ListTenantsByResourceGroupAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName)
        {
            this.LogActionBegin(
                requestId,
                string.Empty);

            var tenants = this.store.ListTenants(
                subscriptionId,
                resourceGroupName);

            foreach (var tenant in tenants)
            {
                await this.SetTenantAsync(requestId, tenant);
            }

            this.LogActionEnd(requestId);
            return tenants.ToArray();
        }

        public async Task<AccountKey> ResetKeyAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            AccountKey accountKey)
        {
            this.LogActionBegin(
                requestId,
                accountName);

            var updated = await this.store.ResetKeyAsync(
                subscriptionId,
                resourceGroupName,
                accountName,
                accountKey,
                MaxRetryForConcurrency);

            await this.SetTenantAsync(requestId, updated);

            var rule = updated.TenantDescription.AuthenticationRules.Single(r => string.Equals(r.KeyName, accountKey.Name, StringComparison.OrdinalIgnoreCase));
            var result = new AccountKey
            {
                Name = accountKey.Name,
                IsPrimaryKey = accountKey.IsPrimaryKey,
                Value = accountKey.IsPrimaryKey ? rule.PrimaryKey : rule.SecondaryKey
            };

            this.LogActionEnd(requestId);
            return result;
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
            this.LogActionBegin(
                requestId,
                accountName);

            var updated = await this.store.CreateOrUpdateChannelAsync(
                subscriptionId,
                resourceGroupName,
                accountName,
                channelName,
                channelType,
                channelFunctions,
                credentials,
                MaxRetryForConcurrency);

            await this.SetTenantAsync(requestId, updated);
            var result = updated.TenantDescription.ChannelSettings.Single(c => string.Equals(c.Name, channelName, StringComparison.OrdinalIgnoreCase));

            this.LogActionEnd(requestId);
            return result;
        }

        public async Task DeleteChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName)
        {
            this.LogActionBegin(
                requestId,
                accountName);

            var updated = await this.store.DeleteChannelAsync(
                subscriptionId,
                resourceGroupName,
                accountName,
                channelName,
                MaxRetryForConcurrency);

            await this.SetTenantAsync(requestId, updated);

            this.LogActionEnd(requestId);
        }

        public async Task<bool> AccountExistsAsync(
            string requestId,
            string accountName)
        {
            this.LogActionBegin(
                requestId,
                accountName);

            var result = await this.store.AccountExistsAsync(
                accountName);

            this.LogActionEnd(requestId);
            return result;
        }
        #endregion

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener((c) =>
                {
                    return new FabricTransportServiceRemotingListener(c, this, null, new ServiceRemotingJsonSerializationProvider());
                })
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        /// <returns>Async task</returns>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var factory = new AdminStoreFactory(this.configuration);
            this.store = factory.GetStore();
            this.cache = new RedisClient(
                this.configuration.TenantCache_DefaultConnectionString,
                this.configuration.TenantCache_DatabaseId);

            var quotaCache = new RedisClient(
                this.configuration.TenantCache_DefaultConnectionString,
                this.configuration.TenantCache_QuotaDatabaseId);
            this.quotaManager = new QuotaManager(
                this.Context,
                this.store,
                quotaCache);

            while (!cancellationToken.IsCancellationRequested)
            {
                var trackingId = Guid.NewGuid().ToString();

                try
                {
                    await this.quotaManager.SynchronizeAsync(trackingId);
                }
                catch (Exception ex)
                {
                    TenantManagementEventSource.Current.TraceException(
                        trackingId,
                        this.Context.NodeContext.NodeName,
                        "Exception raised in synchronize loop",
                        ex);
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private async Task PublishTenantChangedEvent(
            TenantChangedEventType type,
            string tenantName,
            Tenant updatedTenant)
        {
            var eventArgs = new TenantChangedEventArgs
            {
                EventType = type,
                TenantName = tenantName,
                UpdatedTenant = updatedTenant
            };

            var json = JsonConvert.SerializeObject(eventArgs);
            await this.cache.PublishAsync(typeof(TenantChangedEventArgs).Name, json);
        }

        private void LogActionBegin(
            string trackingId,
            string accountName,
            string message = null,
            [CallerMemberName] string action = null)
        {
            TenantManagementEventSource.Current.ActionBegin(
                trackingId,
                action ?? string.Empty,
                accountName ?? string.Empty,
                message ?? string.Empty);
        }

        private void LogActionEnd(
            string trackingId,
            [CallerMemberName] string action = null)
        {
            TenantManagementEventSource.Current.ActionEnd(
                trackingId,
                action ?? string.Empty);
        }

        private async Task SetTenantAsync(
            string trackingId,
            Tenant tenant,
            [CallerMemberName] string action = null)
        {
            await this.cache.SetAsync(tenant.AccountName, tenant);
            TenantManagementEventSource.Current.TenantCacheSet(
                trackingId,
                action ?? string.Empty,
                tenant.AccountName);

            await this.PublishTenantChangedEvent(TenantChangedEventType.TenantUpdated, tenant.AccountName, tenant);
            TenantManagementEventSource.Current.TenantChangePublish(
                trackingId,
                action ?? string.Empty,
                tenant.AccountName,
                TenantChangedEventType.TenantUpdated.ToString());
        }

        private async Task DeleteTenantAsync(
            string trackingId,
            string tenantName,
            [CallerMemberName] string action = null)
        {
            await this.cache.DeleteAsync(tenantName);
            TenantManagementEventSource.Current.TenantCacheDelete(
                trackingId,
                action ?? string.Empty,
                tenantName);

            await this.PublishTenantChangedEvent(TenantChangedEventType.TenantDeleted, tenantName, null);
            TenantManagementEventSource.Current.TenantChangePublish(
                trackingId,
                action ?? string.Empty,
                tenantName,
                TenantChangedEventType.TenantDeleted.ToString());
        }
    }
}
