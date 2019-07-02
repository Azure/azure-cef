// <copyright file="QuotaManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Cache;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.Azure.EngagementFabric.TenantCacheService.Store;
using QuotaMetadataDescription = Microsoft.Azure.EngagementFabric.TenantCacheService.Quota.QuotaMetadata.QuotaMetadataDescription;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.Quota
{
    internal class QuotaManager : IQuotaManager
    {
        private static readonly QuotaOperationResult OK = new QuotaOperationResult(HttpStatusCode.OK, 0);
        private static readonly TimeSpan DefaultCacheTTL = TimeSpan.FromDays(30);
        private static readonly long SynchronizeInterval = (long)TimeSpan.FromMinutes(5).TotalSeconds;
        private static readonly TimeSpan LockTTL = TimeSpan.FromMinutes(5);

        private readonly StatelessServiceContext context;
        private readonly IAdminStore store;
        private readonly RedisClient cache;

        /// <summary>
        /// The in-memory cache contains quota meta-data
        /// </summary>
        private readonly ConcurrentDictionary<string, QuotaMetadata> quotaCache = new ConcurrentDictionary<string, QuotaMetadata>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, QuotaMetadataDescription> quotaMetadataDescriptions = new Dictionary<string, QuotaMetadataDescription>
        {
            {
                Constants.SocialLoginMAU,
                new QuotaMetadataDescription(
                    date => $"{date.Year:d04}{date.Month:d02}",
                    TimeSpan.FromDays(30))
            },
            {
                Constants.SmsSignatureMAUNamingRegex,
                new QuotaMetadataDescription(
                    date => $"{date.Year:d04}{date.Month:d02}{date.Day:d02}",
                    TimeSpan.FromDays(3))
            }
        };

        public QuotaManager(
            StatelessServiceContext context,
            IAdminStore store,
            RedisClient cache)
        {
            this.context = context;
            this.store = store;
            this.cache = cache;
        }

        /// <summary>
        /// Create or update a quota
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <param name="quotaName">Quota name</param>
        /// <param name="quota">Quota count</param>
        /// <returns>Throw exception on failure</returns>
        public async Task CreateOrUpdateQuotaAsync(
            string accountName,
            string quotaName,
            int quota)
        {
            var existing = await this.store.GetQuotaAsync(accountName, quotaName);
            var metadata = this.GetOrCreateQuotaMetadata(accountName, quotaName);

            if (existing == null)
            {
                // Create

                // There might be case that cache exist (if created but deleted before)
                // Update cache to avoid the wrong value is synced into db
                if (await this.cache.ExistAsync(metadata.RemindingKey))
                {
                    await this.cache.SetAsync(metadata.RemindingKey, quota.ToString());
                }

                // Add the record in DB
                // TODO: currently existing metadata will not be update, but the next period (e.g. next day for daily quota) will be updated
                await this.store.CreateOrUpdateQuotaAsync(accountName, quotaName, quota);
            }
            else
            {
                // Update

                // Update the record in DB (for next period) and cache (for current period)
                await this.store.CreateOrUpdateQuotaAsync(accountName, quotaName, quota);

                var reminding = await this.cache.GetIntAsync(metadata.RemindingKey);
                var delta = quota - existing.Quota;
                reminding = delta > 0 ?
                    await this.cache.IncreaseByAsync(metadata.RemindingKey, delta) :
                    await this.cache.DecreaseByAsync(metadata.RemindingKey, 0 - delta);
            }
        }

        /// <summary>
        /// Remove a quota
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <param name="quotaName">Quota name</param>
        /// <returns>Throw exception on failure</returns>
        public async Task RemoveQuotaAsync(
            string accountName,
            string quotaName)
        {
            // Remove in DB for next period
            await this.store.RemoveQuotaAsync(accountName, quotaName);

            // Simply set reminding in Cache as int.MaxValue for current period
            var metadata = this.GetOrCreateQuotaMetadata(accountName, quotaName);
            await this.cache.SetAsync(metadata.RemindingKey, int.MaxValue.ToString());
        }

        /// <summary>
        /// Acquire a quota
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <param name="quotaName">Quota name</param>
        /// <param name="required">Required count for the quota</param>
        /// <returns>The operation result</returns>
        public async Task<QuotaOperationResult> AcquireQuotaAsync(
            string accountName,
            string quotaName,
            int required)
        {
            // New meta-data will be initialized soon by synchronizing with SQL DB
            var metadata = this.GetOrCreateQuotaMetadata(accountName, quotaName);
            if (!metadata.ExistInDB)
            {
                // Return `OK` for nonexistent quota
                return OK;
            }

            // Update the reminding counter to avoid racing condition
            var reminding = await this.cache.DecreaseByAsync(metadata.RemindingKey, required);

            // If the reminding was not in the cache, it will be treated as 0 before decrease. Then
            // the reminding will be a negative number. It is safe to reduce the stress of detecting
            // the initialized flag by limiting it with the case of negative reminding
            if (reminding < 0 && metadata.Initialized)
            {
                metadata.Initialized = await this.cache.ExistAsync(metadata.InitializedKey);
            }

            // By default, return `OK` for enough remaining or uninitialized cache
            var result = OK;
            if (reminding < 0 && metadata.Initialized)
            {
                // Return `NotAcceptable` for exceeded quota after rollback
                reminding = await this.cache.IncreaseByAsync(metadata.RemindingKey, required);
                result = new QuotaOperationResult(HttpStatusCode.NotAcceptable, reminding);
            }

            // Mark the quota as active to be included in next sync
            metadata.MarkAccessed();
            return result;
        }

        /// <summary>
        /// Release a quota
        /// </summary>
        /// <param name="accountName">Account name</param>
        /// <param name="quotaName">Quota name</param>
        /// <param name="released">Released count for the quota</param>
        /// <returns>The operation result</returns>
        public async Task<QuotaOperationResult> ReleaseQuotaAsync(
            string accountName,
            string quotaName,
            int released)
        {
            // New meta-data will be initialized soon by synchronizing with SQL DB
            var metadata = this.GetOrCreateQuotaMetadata(accountName, quotaName);
            if (!metadata.ExistInDB)
            {
                // Return `OK` for nonexistent quota
                return OK;
            }

            // Update the reminding counter. It will always be succeed to release
            await this.cache.IncreaseByAsync(metadata.RemindingKey, released);
            return OK;
        }

        /// <summary>
        /// Synchronize cache with the SQL DB
        /// This method should be called periodically
        /// </summary>
        /// <param name="trackingId">Tracking ID</param>
        /// <returns>n/a</returns>
        public async Task SynchronizeAsync(string trackingId)
        {
            var utcNow = DateTimeOffset.UtcNow;
            var synchronizeTime = utcNow.DateTime;
            var synchronizeUnixTime = utcNow.ToUnixTimeSeconds();

            // Remove expired quotas
            var expiredQuotaIds = this.quotaCache.Values
                .Where(i => i.IsExpired(synchronizeTime, synchronizeUnixTime))
                .Select(i => i.QuotaId)
                .ToList();

            foreach (var quotaId in expiredQuotaIds)
            {
                QuotaMetadata unused;
                this.quotaCache.TryRemove(quotaId, out unused);
            }

            // Only uninitialized or changed quotas will be selected for synchronizing
            var activeQuotas = this.quotaCache.Values
                .Where(i => i.ExistInDB && (!i.Initialized || i.NeedSynchronize(synchronizeUnixTime - SynchronizeInterval)))
                .ToList();

            foreach (var metadata in activeQuotas)
            {
                try
                {
                    await this.SynchronizeAsync(trackingId, metadata, synchronizeTime);
                }
                catch (Exception ex)
                {
                    TenantManagementEventSource.Current.TraceException(
                        trackingId,
                        this.context.NodeContext.NodeName,
                        "Exception raised in synchronize loop",
                        ex);
                }
            }
        }

        private async Task SynchronizeAsync(
            string trackingId,
            QuotaMetadata metadata,
            DateTime synchronizeTime)
        {
            // Exclusively synchronize with SQL DB. Skip and retry later if lock was occupied by others
            var lockExpire = DateTime.UtcNow + LockTTL;
            if (!await this.cache.TryAutoLockAsync(metadata.LockKey, LockTTL))
            {
                TenantManagementEventSource.Current.QuotaSyncSkipped(
                    trackingId,
                    this.context.NodeContext.NodeName,
                    metadata.QuotaId);
                return;
            }

            try
            {
                metadata.Initialized = await this.cache.ExistAsync(metadata.InitializedKey);
                if (metadata.Initialized)
                {
                    // Capture current version number. Any change happened after this will be handled by next interval
                    var versionNumber = metadata.VersionNumber;

                    // Persist cached reminding to SQL DB
                    var reminding = await this.cache.GetIntAsync(metadata.RemindingKey);
                    await this.store.PushQuotaRemindingAsync(
                        metadata,
                        reminding,
                        synchronizeTime);

                    TenantManagementEventSource.Current.QuotaPush(
                        trackingId,
                        this.context.NodeContext.NodeName,
                        metadata.QuotaId,
                        reminding);

                    metadata.MarkSynchronizeCompleted(versionNumber);
                }
                else
                {
                    // Initialize the cached reminding by adding the value from SQL DB, then set the initialized flags
                    var remindingInDB = await this.store.PullQuotaRemindingAsync(
                        metadata);

                    var reminding = await this.cache.IncreaseByAsync(
                        metadata.RemindingKey,
                        remindingInDB);

                    await this.cache.SetAsync(
                        metadata.InitializedKey,
                        DateTime.UtcNow.ToString());

                    metadata.Initialized = true;

                    TenantManagementEventSource.Current.QuotaPull(
                        trackingId,
                        this.context.NodeContext.NodeName,
                        metadata.QuotaId,
                        remindingInDB,
                        reminding);
                }

                var ttl = metadata.MetadataDescription != null ? metadata.MetadataDescription.CacheTTL : QuotaManager.DefaultCacheTTL;
                await this.cache.ExpireAsync(metadata.RemindingKey, ttl);
                await this.cache.ExpireAsync(metadata.InitializedKey, ttl);
            }
            catch (ResourceNotFoundException)
            {
                metadata.ExistInDB = false;

                TenantManagementEventSource.Current.QuotaNotFound(
                    trackingId,
                    this.context.NodeContext.NodeName,
                    metadata.QuotaId);
            }
            finally
            {
                // Always unlock unless timeout
                if (DateTime.UtcNow < lockExpire)
                {
                    await this.cache.ReleaseAutoLockAsync(metadata.LockKey);
                }
                else
                {
                    TenantManagementEventSource.Current.QuotaSyncTimeout(
                        trackingId,
                        this.context.NodeContext.NodeName,
                        metadata.QuotaId);
                }
            }
        }

        private QuotaMetadataDescription GetQuotaMetadataDescription(string quotaName)
        {
            foreach (var description in this.quotaMetadataDescriptions)
            {
                if (Regex.IsMatch(quotaName, description.Key))
                {
                    return description.Value;
                }
            }

            return null;
        }

        private QuotaMetadata GetOrCreateQuotaMetadata(
            string accountName,
            string quotaName)
        {
            string slotId;
            string quotaId;

            var description = this.GetQuotaMetadataDescription(quotaName);
            if (description != null)
            {
                slotId = description.SlotIdGenerator(DateTime.UtcNow);
                quotaId = $"{accountName}/{quotaName}-{slotId}";
            }
            else
            {
                slotId = null;
                quotaId = $"{accountName}/{quotaName}";
            }

            return this.quotaCache.GetOrAdd(
                quotaId,
                id => new QuotaMetadata(
                    id,
                    accountName,
                    quotaName,
                    slotId,
                    description));
        }
    }
}
