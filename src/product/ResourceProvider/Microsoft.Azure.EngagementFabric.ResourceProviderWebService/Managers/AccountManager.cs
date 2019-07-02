// <copyright file="AccountManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Authorize;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Store;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.TenantCache;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers
{
    internal class AccountManager : IAccountManager
    {
        private readonly IResourceProviderStore store;
        private readonly FullTenantCacheClient tenantCacheClient;

        public AccountManager(IResourceProviderStore store)
        {
            this.store = store;
            this.tenantCacheClient = FullTenantCacheClient.GetClient(true);
        }

        public async Task<Account> CreateOrUpdateAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            Account account)
        {
            await this.ValidateSubscriptionRegistration(subscriptionId);

            string message;
            if (!ValidateAccountName(accountName, out message))
            {
                throw new InvalidArgumentException($"Invalid account name: {message}");
            }

            var skuDescription = SkuStore.Descriptions.FirstOrDefault(description => string.Equals(description.Name, account.SKU.Name, StringComparison.OrdinalIgnoreCase));
            if (skuDescription == null)
            {
                throw new InvalidArgumentException($"SKU {account.SKU.Name} is invalid. Supported SKUs are {string.Join(", ", SkuStore.Descriptions.Select(d => d.Name))}");
            }

            var normalizedLocation = account.Location
                .Replace(" ", string.Empty)
                .ToLowerInvariant();
            if (!skuDescription.Locations.Contains(normalizedLocation, StringComparer.OrdinalIgnoreCase))
            {
                throw new InvalidArgumentException($"Location {account.Location} is invalid for SKU {account.SKU.Name}. Supported locations are {string.Join(", ", skuDescription.Locations)}");
            }

            var sku = new SKU
            {
                Name = skuDescription.Name,
                Tier = skuDescription.Tier
            };

            IReadOnlyDictionary<string, int> quotas;
            if (!SkuStore.Quotas.TryGetValue(skuDescription.Name, out quotas))
            {
                quotas = new Dictionary<string, int>();
            }

            var paramTenant = new Tenant
            {
                SubscriptionId = subscriptionId,
                ResourceGroupName = resourceGroupName,
                AccountName = accountName,
                Location = normalizedLocation,
                SKU = sku.Name,
                Tags = account.Tags ?? new Dictionary<string, string>(),
                State = TenantState.Active,
                ResourceId = ResourceIdHelper.GetAccountId(
                    subscriptionId,
                    resourceGroupName,
                    accountName)
            };

            var tenant = await this.tenantCacheClient.CreateOrUpdateTenantAsync(
                requestId,
                paramTenant,
                DefaultSASKeys.DefaultKeyNames.Select(name => new AuthenticationRule
                {
                    KeyName = name,
                    PrimaryKey = SASHelper.GenerateKey(DefaultSASKeys.DefaultKeyLength),
                    SecondaryKey = SASHelper.GenerateKey(DefaultSASKeys.DefaultKeyLength),
                }).ToArray(),
                quotas.ToDictionary(pair => pair.Key, pair => pair.Value));

            // Check subscription registration state to handle racing condition
            if (!await this.store.IsSubscriptionRegisteredAsync(subscriptionId))
            {
                tenant.IsDisabled = true;
                await this.tenantCacheClient.UpdateTenantAsync(
                    requestId,
                    tenant);
            }

            return await Task.FromResult(new Account
            {
                Id = ResourceIdHelper.GetAccountId(
                    tenant.SubscriptionId,
                    tenant.ResourceGroupName,
                    tenant.AccountName),
                Name = tenant.AccountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = tenant.Location,
                SKU = SkuStore.GetSKU(tenant.SKU),
                Tags = tenant.Tags
            });
        }

        public async Task<Account> UpdateAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            AccountPatch accountPatch)
        {
            await this.ValidateSubscriptionRegistration(subscriptionId);

            var paramTenant = new Tenant
            {
                SubscriptionId = subscriptionId,
                ResourceGroupName = resourceGroupName,
                AccountName = accountName,
                Tags = accountPatch.Tags ?? new Dictionary<string, string>(),
                State = TenantState.Active
            };

            var tenant = await this.tenantCacheClient.UpdateTenantAsync(
                requestId,
                paramTenant);

            return await Task.FromResult(new Account
            {
                Id = ResourceIdHelper.GetAccountId(
                    tenant.SubscriptionId,
                    tenant.ResourceGroupName,
                    tenant.AccountName),
                Name = tenant.AccountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = tenant.Location,
                SKU = SkuStore.GetSKU(tenant.SKU),
                Tags = tenant.Tags
            });
        }

        public async Task<bool> DeleteAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            return await this.tenantCacheClient.DeleteTenantAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName);
        }

        public async Task<Account> GetAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            var tenant = await this.tenantCacheClient.GetTenantAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName);

            return new Account
            {
                Id = ResourceIdHelper.GetAccountId(
                    tenant.SubscriptionId,
                    tenant.ResourceGroupName,
                    tenant.AccountName),
                Name = tenant.AccountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = tenant.Location,
                SKU = SkuStore.GetSKU(tenant.SKU),
                Tags = tenant.Tags
            };
        }

        public async Task<IEnumerable<Account>> ListAccountsAsync(
            string requestId,
            string subscriptionId)
        {
            var tenants = await this.tenantCacheClient.ListTenantsAsync(
                requestId,
                subscriptionId);

            return tenants.Select(tenant => new Account
            {
                Id = ResourceIdHelper.GetAccountId(
                    tenant.SubscriptionId,
                    tenant.ResourceGroupName,
                    tenant.AccountName),
                Name = tenant.AccountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = tenant.Location,
                SKU = SkuStore.GetSKU(tenant.SKU),
                Tags = tenant.Tags
            });
        }

        public async Task<IEnumerable<Account>> ListAccountsByResourceGroupAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName)
        {
            var tenants = await this.tenantCacheClient.ListTenantsAsync(
                requestId,
                subscriptionId,
                resourceGroupName);

            return tenants.Select(tenant => new Account
            {
                Id = ResourceIdHelper.GetAccountId(
                    tenant.SubscriptionId,
                    tenant.ResourceGroupName,
                    tenant.AccountName),
                Name = tenant.AccountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = tenant.Location,
                SKU = SkuStore.GetSKU(tenant.SKU),
                Tags = tenant.Tags
            });
        }

        public async Task<IEnumerable<KeyDescription>> ListKeysAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            await this.ValidateSubscriptionRegistration(subscriptionId);

            var tenant = await this.tenantCacheClient.GetTenantAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName);

            return tenant.TenantDescription.AuthenticationRules.SelectMany(r => new[]
            {
                new KeyDescription
                {
                    Name = r.KeyName,
                    Rank = KeyRank.PrimaryKey,
                    Value = r.PrimaryKey
                },
                new KeyDescription
                {
                    Name = r.KeyName,
                    Rank = KeyRank.SecondaryKey,
                    Value = r.SecondaryKey
                }
            });
        }

        public async Task<KeyDescription> RegenerateKeyAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            RegenerateKeyParameter parameter)
        {
            await this.ValidateSubscriptionRegistration(subscriptionId);

            var accountKey = new AccountKey
            {
                Name = parameter.Name,
                IsPrimaryKey = parameter.Rank == KeyRank.PrimaryKey,
                Value = SASHelper.GenerateKey(DefaultSASKeys.DefaultKeyLength),
            };

            var updated = await this.tenantCacheClient.ResetKeyAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName,
                accountKey);

            return new KeyDescription
            {
                Name = updated.Name,
                Rank = updated.IsPrimaryKey ? KeyRank.PrimaryKey : KeyRank.SecondaryKey,
                Value = updated.Value
            };
        }

        public async Task<Channel> CreateOrUpdateChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            Channel channel)
        {
            await this.ValidateSubscriptionRegistration(subscriptionId);

            string message;
            if (!ValidateChannelName(channelName, out message))
            {
                throw new InvalidArgumentException($"Invalid account name: {message}");
            }

            var updated = await this.tenantCacheClient.CreateOrUpdateChannelAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName,
                channelName,
                channel.Properties.ChannelType,
                channel.Properties.ChannelFunctions?.ToArray(),
                channel.Properties.Credentials);

            return new Channel
            {
                Id = ResourceIdHelper.GetChannelId(
                    subscriptionId,
                    resourceGroupName,
                    accountName,
                    updated.Name),
                Name = updated.Name,
                Type = NameStore.FullyQualifiedChannelResourceType,
                Properties = new ChannelProperties
                {
                    ChannelType = updated.Type,
                    ChannelFunctions = updated.Functions,
                    Credentials = MaskCredentials(updated.Type, updated.Credentials)
                }
            };
        }

        public async Task<bool> DeleteChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName)
        {
            return await this.tenantCacheClient.DeleteChannelAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName,
                channelName);
        }

        public async Task<Channel> GetChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName)
        {
            var tenant = await this.tenantCacheClient.GetTenantAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName);

            var channel = tenant.TenantDescription.ChannelSettings.SingleOrDefault(c => string.Equals(c.Name, channelName, StringComparison.OrdinalIgnoreCase));
            if (channel == null)
            {
                throw new ResourceNotFoundException($"Can not find channel '{channelName}' in account '{accountName}'");
            }

            return new Channel
            {
                Id = ResourceIdHelper.GetChannelId(
                    subscriptionId,
                    resourceGroupName,
                    accountName,
                    channel.Name),
                Name = channel.Name,
                Type = NameStore.FullyQualifiedChannelResourceType,
                Properties = new ChannelProperties
                {
                    ChannelType = channel.Type,
                    ChannelFunctions = channel.Functions,
                    Credentials = MaskCredentials(channel.Type, channel.Credentials)
                }
            };
        }

        public async Task<IEnumerable<Channel>> ListChannelsByAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            var tenant = await this.tenantCacheClient.GetTenantAsync(
                requestId,
                subscriptionId,
                resourceGroupName,
                accountName);

            return tenant.TenantDescription.ChannelSettings.Select(channel => new Channel
            {
                Id = ResourceIdHelper.GetChannelId(
                    subscriptionId,
                    resourceGroupName,
                    accountName,
                    channel.Name),
                Name = channel.Name,
                Type = NameStore.FullyQualifiedChannelResourceType,
                Properties = new ChannelProperties
                {
                    ChannelType = channel.Type,
                    ChannelFunctions = channel.Functions,
                    Credentials = MaskCredentials(channel.Type, channel.Credentials)
                }
            });
        }

        public async Task<CheckNameAvailabilityResult> CheckNameAvailabilityAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string resourceType,
            string resourceName)
        {
            await this.ValidateSubscriptionRegistration(subscriptionId);

            if (!string.Equals(resourceType, NameStore.FullyQualifiedAccountResourceType, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidArgumentException($"Invalid resource type");
            }

            string message;
            if (!ValidateAccountName(resourceName, out message))
            {
                return new CheckNameAvailabilityResult
                {
                    NameAvailabile = false,
                    Reason = CheckNameUnavailableReason.Invalid,
                    Message = message
                };
            }

            if (await this.tenantCacheClient.AccountExistsAsync(requestId, resourceName))
            {
                return new CheckNameAvailabilityResult
                {
                    NameAvailabile = false,
                    Reason = CheckNameUnavailableReason.AlreadyExists,
                    Message = $"Account '{resourceName}' already exists"
                };
            }

            return new CheckNameAvailabilityResult
            {
                NameAvailabile = true
            };
        }

        private static bool ValidateAccountName(string accountName, out string message)
        {
            if (!Regex.IsMatch(accountName, "^[A-Za-z][A-Za-z0-9]{2,23}$"))
            {
                message = $"The account name must be 3 to 24 letters and numbers and the first character must be letter";
                return false;
            }

            message = null;
            return true;
        }

        private static bool ValidateChannelName(string channelName, out string message)
        {
            if (!Regex.IsMatch(channelName, "^[A-Za-z][A-Za-z0-9]{2,23}$"))
            {
                message = $"The account name must be 3 to 24 letters and numbers and the first character must be letter";
                return false;
            }

            message = null;
            return true;
        }

        private static Dictionary<string, string> MaskCredentials(string channelType, Dictionary<string, string> credentials)
        {
            var result = new Dictionary<string, string>(credentials);

            IEnumerable<string> maskFreeKeys;
            if (ChannelTypeStore.MaskFreeCredentialKeys.TryGetValue(channelType, out maskFreeKeys))
            {
                var maskKeys = result.Keys
                    .Except(maskFreeKeys, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                foreach (var key in maskKeys)
                {
                    result[key] = string.Empty;
                }
            }

            return result;
        }

        private async Task ValidateSubscriptionRegistration(string subscriptionId)
        {
            if (!await this.store.IsSubscriptionRegisteredAsync(subscriptionId))
            {
                throw new InvalidArgumentException($"Subscription {subscriptionId} is not registered");
            }
        }
    }
}
