// <copyright file="AdminStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.Azure.EngagementFabric.TenantCacheService.EntityFramework;
using Microsoft.Azure.EngagementFabric.TenantCacheService.Quota;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.Store
{
    internal class AdminStore : IAdminStore
    {
        private readonly string connectionString;

        public AdminStore(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var entityStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                Metadata = "res://*/EntityFramework.AdminStoreDataModel.csdl|res://*/EntityFramework.AdminStoreDataModel.ssdl|res://*/EntityFramework.AdminStoreDataModel.msl",
                ProviderConnectionString = connectionStringBuilder.ToString()
            };

            this.connectionString = entityStringBuilder.ConnectionString;
        }

        public async Task<TenantEntity> GetTenantAsync(string engagementAccount)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                return await ctx.Tenants.SingleOrDefaultAsync(t => t.AccountName == engagementAccount);
            }
        }

        #region Quota

        public async Task<QuotaEntity> GetQuotaAsync(
            string engagementAccount,
            string quotaName)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                return await ctx.Quotas.SingleOrDefaultAsync(
                    q => q.AccountName == engagementAccount &&
                    q.QuotaName == quotaName);
            }
        }

        public async Task<QuotaEntity> CreateOrUpdateQuotaAsync(
            string engagementAccount,
            string quotaName,
            int quota)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                var existing = await ctx.Quotas.SingleOrDefaultAsync(
                    q => q.AccountName == engagementAccount &&
                    q.QuotaName == quotaName);

                var now = DateTime.UtcNow;
                if (existing != null)
                {
                    existing.Quota = quota;
                    existing.LastUpdatedTime = now;
                }
                else
                {
                    existing = new QuotaEntity
                    {
                        AccountName = engagementAccount,
                        QuotaName = quotaName,
                        Remaining = quota,
                        Quota = quota,
                        CreatedTime = now,
                        LastUpdatedTime = now
                    };
                    ctx.Quotas.Add(existing);
                }

                await ctx.SaveChangesAsync();
                return existing;
            }
        }

        public async Task RemoveQuotaAsync(
            string engagementAccount,
            string quotaName)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                var existing = await ctx.Quotas.SingleOrDefaultAsync(
                    q => q.AccountName == engagementAccount &&
                    q.QuotaName == quotaName);

                if (existing != null)
                {
                    ctx.Quotas.Remove(existing);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task<int> PullQuotaRemindingAsync(
            QuotaMetadata metadata)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                var entity = await ctx.Quotas.SingleOrDefaultAsync(t => t.AccountName == metadata.AccountName && t.QuotaName == metadata.QuotaName);
                if (entity == null)
                {
                    throw new ResourceNotFoundException($"No quota {metadata.AccountName}/{metadata.QuotaName}");
                }

                return metadata.IsCurrentSlot(entity.LastUpdatedTime) ? entity.Remaining : entity.Quota;
            }
        }

        public async Task PushQuotaRemindingAsync(
            QuotaMetadata metadata,
            int reminding,
            DateTime synchronizeTime)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                var entity = await ctx.Quotas.SingleOrDefaultAsync(t => t.AccountName == metadata.AccountName && t.QuotaName == metadata.QuotaName);
                if (entity == null)
                {
                    throw new ResourceNotFoundException($"No quota {metadata.AccountName}/{metadata.QuotaName}");
                }

                if (metadata.IsCurrentOrNewerSlot(entity.LastUpdatedTime))
                {
                    entity.Remaining = reminding;
                    entity.LastUpdatedTime = synchronizeTime;
                    await ctx.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region Resource provider methods
        public async Task<Tenant> CreateOrUpdateTenantAsync(
            Tenant tenant,
            IEnumerable<AuthenticationRule> authenticationRules,
            IReadOnlyDictionary<string, int> quotas)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                var now = DateTime.UtcNow;
                var entity = await ctx.Tenants.GetAsync(
                    tenant.SubscriptionId,
                    tenant.ResourceGroupName,
                    tenant.AccountName,
                    false);

                if (entity == null)
                {
                    entity = new TenantEntity
                    {
                        SubscriptionId = tenant.SubscriptionId,
                        ResourceGroup = tenant.ResourceGroupName,
                        AccountName = tenant.AccountName,
                        Location = tenant.Location,
                        SKU = tenant.SKU,
                        Tags = JsonConvert.SerializeObject(tenant.Tags),
                        State = tenant.State.ToString(),
                        Created = now,
                        LastModified = now,
                        Address = tenant.Address,
                        ResourceDescription = JsonConvert.SerializeObject(new TenantDescription
                        {
                            AuthenticationRules = authenticationRules.ToList(),
                            ChannelSettings = new List<ChannelSetting>()
                        }),
                        ResourceId = tenant.ResourceId
                    };
                    ctx.Tenants.Add(entity);

                    ctx.Quotas.AddRange(quotas.Select(pair => new QuotaEntity
                    {
                        AccountName = tenant.AccountName,
                        QuotaName = pair.Key,
                        Remaining = pair.Value,
                        Quota = pair.Value,
                        CreatedTime = now,
                        LastUpdatedTime = now
                    }));
                }
                else
                {
                    // Update-able properties: Tags, State and Address
                    entity.Tags = JsonConvert.SerializeObject(tenant.Tags);
                    entity.State = tenant.State.ToString();
                    entity.Address = tenant.Address;
                    entity.LastModified = now;
                }

                await ctx.SaveChangesAsync();
                return entity.ToTenant();
            }
        }

        public async Task<Tenant> UpdateTenantAsync(
            Tenant tenant)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                var entity = await ctx.Tenants.GetAsync(
                    tenant.SubscriptionId,
                    tenant.ResourceGroupName,
                    tenant.AccountName);

                // Update-able properties: Tags, State and Address
                entity.Tags = JsonConvert.SerializeObject(tenant.Tags);
                entity.State = tenant.State.ToString();
                entity.Address = tenant.Address;
                entity.LastModified = DateTime.UtcNow;

                if (tenant.IsDisabled.HasValue)
                {
                    entity.IsDisabled = tenant.IsDisabled;
                }

                await ctx.SaveChangesAsync();
                return entity.ToTenant();
            }
        }

        public async Task DeleteTenantAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                var entity = await ctx.Tenants.GetAsync(
                    subscriptionId,
                    resourceGroupName,
                    accountName);
                ctx.Tenants.Remove(entity);

                var quotaEntities = ctx.Quotas.Where(e => e.AccountName == accountName);
                ctx.Quotas.RemoveRange(quotaEntities);

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<Tenant> GetTenantAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                var entity = await ctx.Tenants.GetAsync(
                    subscriptionId,
                    resourceGroupName,
                    accountName);

                return entity.ToTenant();
            }
        }

        public IEnumerable<Tenant> ListTenants(
            string subscriptionId)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                return ctx.Tenants
                    .Where(e =>
                        e.SubscriptionId == subscriptionId)
                    .ToList()
                    .Select(e => e.ToTenant());
            }
        }

        public IEnumerable<Tenant> ListTenants(
            string subscriptionId,
            string resourceGroupName)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                return ctx.Tenants
                    .Where(e =>
                        e.SubscriptionId == subscriptionId
                        && e.ResourceGroup == resourceGroupName)
                    .ToList()
                    .Select(e => e.ToTenant());
            }
        }

        public async Task<Tenant> ResetKeyAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            AccountKey accountKey,
            int maxRetry)
        {
            return await this.WithTenantDescriptionAsync(
                subscriptionId,
                resourceGroupName,
                accountName,
                description =>
                {
                    var rule = description.AuthenticationRules.SingleOrDefault(r => string.Equals(r.KeyName, accountKey.Name, StringComparison.OrdinalIgnoreCase));
                    if (rule == null)
                    {
                        throw new ResourceNotFoundException($"Can not find key '{accountKey.Name}' in account '{accountName}'");
                    }

                    if (accountKey.IsPrimaryKey)
                    {
                        rule.PrimaryKey = accountKey.Value;
                    }
                    else
                    {
                        rule.SecondaryKey = accountKey.Value;
                    }
                },
                maxRetry);
        }

        public async Task<Tenant> CreateOrUpdateChannelAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            string channelType,
            IEnumerable<string> channelFunctions,
            Dictionary<string, string> credentials,
            int maxRetry)
        {
            return await this.WithTenantDescriptionAsync(
                subscriptionId,
                resourceGroupName,
                accountName,
                description =>
                {
                    if (description.ChannelSettings.Any(c =>
                        string.Equals(c.Type, channelType, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(c.Name, channelName, StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new ResourceAlreadyExistsException($"A channel with type '{channelType}' already exists");
                    }

                    var channel = description.ChannelSettings.SingleOrDefault(c => string.Equals(c.Name, channelName, StringComparison.OrdinalIgnoreCase));
                    if (channel == null)
                    {
                        description.ChannelSettings.Add(new ChannelSetting
                        {
                            Name = channelName,
                            Type = channelType,
                            Functions = channelFunctions,
                            Credentials = credentials
                        });
                    }
                    else
                    {
                        channel.Functions = channelFunctions;

                        foreach (var key in credentials.Keys.ToList())
                        {
                            if (credentials[key] == null)
                            {
                                if (channel.Credentials.TryGetValue(key, out var value))
                                {
                                    credentials[key] = value;
                                }
                            }
                        }

                        channel.Credentials = credentials;
                    }
                },
                maxRetry);
        }

        public async Task<Tenant> DeleteChannelAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            int maxRetry)
        {
            return await this.WithTenantDescriptionAsync(
                subscriptionId,
                resourceGroupName,
                accountName,
                description =>
                {
                    var channel = description.ChannelSettings.SingleOrDefault(c => string.Equals(c.Name, channelName, StringComparison.OrdinalIgnoreCase));
                    if (channel == null)
                    {
                        throw new ResourceNotFoundException($"Can not find channel '{channelName}' in account '{accountName}'");
                    }

                    description.ChannelSettings.Remove(channel);
                },
                maxRetry);
        }

        public async Task<bool> AccountExistsAsync(
            string accountName)
        {
            using (var ctx = new AdminEntities(this.connectionString))
            {
                return await ctx.Tenants
                    .AnyAsync(e =>
                        e.AccountName == accountName);
            }
        }

        private async Task<Tenant> WithTenantDescriptionAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            Action<TenantDescription> action,
            int maxRetry)
        {
            var retry = 0;

            while (true)
            {
                using (var ctx = new AdminEntities(this.connectionString))
                {
                    var entity = await ctx.Tenants.GetAsync(
                        subscriptionId,
                        resourceGroupName,
                        accountName);

                    var description = JsonConvert.DeserializeObject<TenantDescription>(entity.ResourceDescription);

                    action(description);

                    entity.ResourceDescription = JsonConvert.SerializeObject(description);
                    entity.LastModified = DateTime.UtcNow;

                    try
                    {
                        await ctx.SaveChangesAsync();
                        return entity.ToTenant();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (++retry >= maxRetry)
                        {
                            throw new ConcurrencyException("Failed due to concurrency update on the same entity. Please try it again later");
                        }
                    }
                }
            }
        }
        #endregion
    }
}
