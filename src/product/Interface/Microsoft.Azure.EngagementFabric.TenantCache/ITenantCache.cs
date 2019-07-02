// -----------------------------------------------------------------------
// <copyright file="ITenantCache.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Microsoft.Azure.EngagementFabric.TenantCache
{
    public interface ITenantCache : IService
    {
        Task<TenantCacheConfiguration> GetCacheConfiguration();

        Task<Tenant> GetTenantAsyncInternal(string engagementAccount);
         
        Task<QuotaOperationResult> AcquireQuotaAsync(
            string engagementAccount,
            string quotaName,
            int required);

        Task<QuotaOperationResult> ReleaseQuotaAsync(
            string engagementAccount,
            string quotaName,
            int released);

        /// <summary>
        /// Create or update a quota for an account
        /// </summary>
        /// <param name="engagementAccount">Engagement Account</param>
        /// <param name="quotaName">Quota Name</param>
        /// <param name="quota">Quota Count</param>
        /// <returns>N/A, throw exception if failed</returns>
        Task CreateOrUpdateQuotaAsync(
            string engagementAccount,
            string quotaName,
            int quota);

        /// <summary>
        /// Remove a quota for an account
        /// </summary>
        /// <param name="engagementAccount">Engagement Account</param>
        /// <param name="quotaName">Quota Name</param>
        /// <returns>N/A, throw exception if failed</returns>
        Task RemoveQuotaAsync(
            string engagementAccount,
            string quotaName);

        #region Resource provider methods
        Task<Tenant> CreateOrUpdateTenantAsync(
            string requestId,
            Tenant tenant,
            AuthenticationRule[] authenticationRules,
            Dictionary<string, int> quotas);

        Task<Tenant> UpdateTenantAsync(
            string requestId,
            Tenant tenant);

        Task DeleteTenantAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName);

        Task<Tenant> GetTenantAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName);

        Task<Tenant[]> ListTenantsAsync(
            string requestId,
            string subscriptionId);

        Task<Tenant[]> ListTenantsByResourceGroupAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName);

        Task<AccountKey> ResetKeyAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            AccountKey authenticationRule);

        Task<ChannelSetting> CreateOrUpdateChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            string channelType,
            string[] channelFunctions,
            Dictionary<string, string> credentials);

        Task DeleteChannelAsync(
            string requestId,
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName);

        Task<bool> AccountExistsAsync(
            string requestId,
            string accountName);
        #endregion
    }
}
