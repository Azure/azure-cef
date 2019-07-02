// <copyright file="IAccountManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers
{
    /// <summary>
    /// The interface for account and channel operations
    /// </summary>
    public interface IAccountManager
    {
        /// <summary>
        /// Create or update account
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <param name="account">Account description</param>
        /// <returns>Created or updated account description</returns>
        Task<Account> CreateOrUpdateAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            Account account);

        /// <summary>
        /// Update account
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <param name="accountPatch">Account patch</param>
        /// <returns>Updated account description</returns>
        Task<Account> UpdateAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            AccountPatch accountPatch);

        /// <summary>
        /// Delete account
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <returns>True if deleted successfully, False if no account found to be deleted</returns>
        Task<bool> DeleteAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName);

        /// <summary>
        /// Get account
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <returns>Account description</returns>
        Task<Account> GetAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName);

        /// <summary>
        /// List account
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <returns>Account descriptions</returns>
        Task<IEnumerable<Account>> ListAccountsAsync(
            string requestId,
            string subscriptionId);

        /// <summary>
        /// List account by resource group
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <returns>Account descriptions</returns>
        Task<IEnumerable<Account>> ListAccountsByResourceGroupAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName);

        /// <summary>
        /// List account keys
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <returns>Account key descriptions</returns>
        Task<IEnumerable<KeyDescription>> ListKeysAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName);

        /// <summary>
        /// Regenerate account key
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <param name="parameter">The parameter specifying key to be regenerated</param>
        /// <returns>Regenerated key description</returns>
        Task<KeyDescription> RegenerateKeyAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            RegenerateKeyParameter parameter);

        /// <summary>
        /// Create or update channel
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <param name="channelName">Channel name</param>
        /// <param name="channel">Channel description</param>
        /// <returns>Created or updated channel description</returns>
        Task<Channel> CreateOrUpdateChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            Channel channel);

        /// <summary>
        /// Delete channel
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <param name="channelName">Channel name</param>
        /// <returns>True if deleted successfully, False if no channel found to be deleted</returns>
        Task<bool> DeleteChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName);

        /// <summary>
        /// Get channel
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <param name="channelName">Channel name</param>
        /// <returns>Channel description</returns>
        Task<Channel> GetChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName);

        /// <summary>
        /// List channel by account
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <returns>Channel descriptions</returns>
        Task<IEnumerable<Channel>> ListChannelsByAccountAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName);

        /// <summary>
        /// Check name availability
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="subscriptionId">Subscription ID</param>
        /// <param name="resourceGroupName">Resource group name</param>
        /// <param name="resourceType">Type of the testing resource</param>
        /// <param name="resourceName">Name of the testing resource</param>
        /// <returns>Availability result</returns>
        Task<CheckNameAvailabilityResult> CheckNameAvailabilityAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string resourceType,
            string resourceName);
    }
}
