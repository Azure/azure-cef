// <copyright file="IAdminStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.Azure.EngagementFabric.TenantCacheService.EntityFramework;
using Microsoft.Azure.EngagementFabric.TenantCacheService.Quota;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.Store
{
    internal interface IAdminStore
    {
        Task<TenantEntity> GetTenantAsync(string engagementAccount);

        #region Quota

        Task<QuotaEntity> GetQuotaAsync(string engagementAccount, string quotaName);

        Task<QuotaEntity> CreateOrUpdateQuotaAsync(string engagementAccount, string quotaName, int quota);

        Task RemoveQuotaAsync(string engagementAccount, string quotaName);

        Task<int> PullQuotaRemindingAsync(
            QuotaMetadata metadata);

        Task PushQuotaRemindingAsync(
            QuotaMetadata metadata,
            int reminding,
            DateTime synchronizeTime);

        #endregion

        #region Resource provider methods
        Task<Tenant> CreateOrUpdateTenantAsync(
            Tenant tenant,
            IEnumerable<AuthenticationRule> authenticationRules,
            IReadOnlyDictionary<string, int> quotas);

        Task<Tenant> UpdateTenantAsync(
            Tenant tenant);

        Task DeleteTenantAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName);

        Task<Tenant> GetTenantAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName);

        IEnumerable<Tenant> ListTenants(
            string subscriptionId);

        IEnumerable<Tenant> ListTenants(
            string subscriptionId,
            string resourceGroupName);

        Task<Tenant> ResetKeyAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            AccountKey accountKey,
            int maxRetry);

        Task<Tenant> CreateOrUpdateChannelAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            string channelType,
            IEnumerable<string> channelFunctions,
            Dictionary<string, string> credentials,
            int maxRetry);

        Task<Tenant> DeleteChannelAsync(
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName,
            int maxRetry);

        Task<bool> AccountExistsAsync(
            string accountName);
        #endregion
    }
}
