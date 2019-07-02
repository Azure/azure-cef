// <copyright file="MockAccountManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers
{
    internal class MockAccountManager : IAccountManager
    {
        public async Task<Account> CreateOrUpdateAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            Account account)
        {
            return await Task.FromResult(new Account
            {
                Id = ResourceIdHelper.GetAccountId(subscriptionId, resourceGroupName, accountName),
                Name = accountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = account.Location,
                SKU = account.SKU,
                Tags = account.Tags
            });
        }

        public async Task<Account> UpdateAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            AccountPatch accountPatch)
        {
            return await Task.FromResult(new Account
            {
                Id = ResourceIdHelper.GetAccountId(subscriptionId, resourceGroupName, accountName),
                Name = accountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = "China North",
                SKU = SkuStore.B1,
                Tags = accountPatch.Tags
            });
        }

        public async Task<bool> DeleteAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            return await Task.FromResult(true);
        }

        public async Task<Account> GetAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            return await Task.FromResult(new Account
            {
                Id = ResourceIdHelper.GetAccountId(subscriptionId, resourceGroupName, accountName),
                Name = accountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = "China North",
                SKU = SkuStore.B1
            });
        }

        public async Task<IEnumerable<Account>> ListAccountsAsync(
            string requestId,
            string subscriptionId)
        {
            var resourceGroupName = "MockResourceGroup";
            var accountNames = new[]
            {
                "MockAccount1",
                "MockAccount2"
            };

            return await Task.FromResult(accountNames.Select(accountName => new Account
            {
                Id = ResourceIdHelper.GetAccountId(subscriptionId, resourceGroupName, accountName),
                Name = accountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = "China North",
                SKU = SkuStore.B1
            }));
        }

        public async Task<IEnumerable<Account>> ListAccountsByResourceGroupAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName)
        {
            var accountNames = new[]
            {
                "MockAccount1",
                "MockAccount2"
            };

            return await Task.FromResult(accountNames.Select(accountName => new Account
            {
                Id = ResourceIdHelper.GetAccountId(subscriptionId, resourceGroupName, accountName),
                Name = accountName,
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = "China North",
                SKU = SkuStore.B1
            }));
        }

        public async Task<IEnumerable<KeyDescription>> ListKeysAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            var keyNames = new[]
            {
                "full",
                "device"
            };

            return await Task.FromResult(keyNames.SelectMany(keyName => new[]
            {
                new KeyDescription
                {
                    Name = keyName,
                    Rank = KeyRank.PrimaryKey,
                    Value = "<key value>"
                },
                new KeyDescription
                {
                    Name = keyName,
                    Rank = KeyRank.SecondaryKey,
                    Value = "<key value>"
                }
            }));
        }

        public async Task<KeyDescription> RegenerateKeyAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            RegenerateKeyParameter parameter)
        {
            return await Task.FromResult(new KeyDescription
            {
                Name = parameter.Name,
                Rank = parameter.Rank,
                Value = "<key value>"
            });
        }

        public async Task<Channel> CreateOrUpdateChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            Channel channel)
        {
            return await Task.FromResult(new Channel
            {
                Id = ResourceIdHelper.GetChannelId(subscriptionId, resourceGroupName, accountName, channelName),
                Name = channelName,
                Type = NameStore.FullyQualifiedChannelResourceType,
                Properties = new ChannelProperties
                {
                    Credentials = new Dictionary<string, string>
                    {
                        { "appId", "<appId>" },
                        { "appKey", string.Empty }
                    }
                }
            });
        }

        public async Task<bool> DeleteChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName)
        {
            return await Task.FromResult(true);
        }

        public async Task<Channel> GetChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName)
        {
            return await Task.FromResult(new Channel
            {
                Id = ResourceIdHelper.GetChannelId(subscriptionId, resourceGroupName, accountName, channelName),
                Name = channelName,
                Type = NameStore.FullyQualifiedChannelResourceType,
                Properties = new ChannelProperties
                {
                    Credentials = new Dictionary<string, string>
                    {
                        { "appId", "<appId>" },
                        { "appKey", string.Empty }
                    }
                }
            });
        }

        public async Task<IEnumerable<Channel>> ListChannelsByAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            var channelNames = new[]
            {
                "MockChannel1",
                "MockChannel2"
            };

            return await Task.FromResult(channelNames.Select(channelName => new Channel
            {
                Id = ResourceIdHelper.GetChannelId(subscriptionId, resourceGroupName, accountName, channelName),
                Name = channelName,
                Type = NameStore.FullyQualifiedChannelResourceType,
                Properties = new ChannelProperties
                {
                    Credentials = new Dictionary<string, string>
                    {
                        { "appId", "<appId>" },
                        { "appKey", string.Empty }
                    }
                }
            }));
        }

        public async Task<CheckNameAvailabilityResult> CheckNameAvailabilityAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string resourceType,
            string resourceName)
        {
            return await Task.FromResult(new CheckNameAvailabilityResult
            {
                NameAvailabile = true
            });
        }
    }
}
