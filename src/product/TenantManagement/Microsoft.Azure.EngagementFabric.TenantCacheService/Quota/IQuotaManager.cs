// <copyright file="IQuotaManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.Quota
{
    internal interface IQuotaManager
    {
        Task CreateOrUpdateQuotaAsync(
            string accountName,
            string quotaName,
            int quota);

        Task RemoveQuotaAsync(
            string accountName,
            string quotaName);

        Task<QuotaOperationResult> AcquireQuotaAsync(
            string accountName,
            string quotaName,
            int required);

        Task<QuotaOperationResult> ReleaseQuotaAsync(
            string accountName,
            string quotaName,
            int released);

        Task SynchronizeAsync(string trackingId);
    }
}
