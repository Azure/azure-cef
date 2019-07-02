// <copyright file="QuotaMetadata.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.Quota
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    internal class QuotaMetadata
    {
        private static readonly long MetadataTTL = (long)TimeSpan.FromDays(90).TotalSeconds;

        private int versionNumber;
        private int lastSynchronizedVersionNumber;

        /// <summary>
        /// Unix time for last synchronize
        /// </summary>
        private long lastSynchronize;

        public QuotaMetadata(
            string quotaId,
            string accountName,
            string quotaName,
            string slotId,
            QuotaMetadataDescription description)
        {
            this.versionNumber = 0;
            this.lastSynchronizedVersionNumber = this.versionNumber - 1;

            this.QuotaId = quotaId;
            this.AccountName = accountName;
            this.QuotaName = quotaName;
            this.SlotId = slotId;
            this.MetadataDescription = description;

            this.lastSynchronize = 0;   // Never synchronized
            this.Initialized = true;    // Assume the cache is initialized
            this.ExistInDB = true;      // Assume exist at the beginning

            this.RemindingKey = $"{this.QuotaId}/reminding";
            this.InitializedKey = $"{this.QuotaId}/initialized";
            this.LockKey = $"{this.QuotaId}/lock";
        }

        public string QuotaId { get; private set; }

        public string AccountName { get; private set; }

        public string QuotaName { get; private set; }

        public string SlotId { get; private set; }

        public QuotaMetadataDescription MetadataDescription { get; private set; }

        /// <summary>
        /// Initialized flag. True means the cached reminding was synchronized with DB. False means the cached
        /// reminding is a off-line patch
        /// </summary>
        public bool Initialized { get; set; }

        /// <summary>
        /// True for quota is not in the SQL DB
        /// </summary>
        public bool ExistInDB { get; set; }

        public int VersionNumber => this.versionNumber;

        public string RemindingKey { get; private set; }

        public string InitializedKey { get; private set; }

        public string LockKey { get; private set; }

        public void MarkAccessed()
        {
            Interlocked.Increment(ref this.versionNumber);
        }

        public bool NeedSynchronize(long upperBoundaryOfLastSynchronizeTime)
        {
            return this.versionNumber != this.lastSynchronizedVersionNumber && this.lastSynchronize < upperBoundaryOfLastSynchronizeTime;
        }

        public void MarkSynchronizeCompleted(int versionNumber)
        {
            this.lastSynchronizedVersionNumber = versionNumber;
            this.lastSynchronize = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public bool IsCurrentSlot(DateTime baseline)
        {
            if (this.SlotId == null || this.MetadataDescription?.SlotIdGenerator == null)
            {
                return true;
            }

            return string.Equals(this.SlotId, this.MetadataDescription.SlotIdGenerator(baseline), StringComparison.OrdinalIgnoreCase);
        }

        public bool IsCurrentOrNewerSlot(DateTime baseline)
        {
            if (this.SlotId == null || this.MetadataDescription?.SlotIdGenerator == null)
            {
                return true;
            }

            return string.Compare(this.SlotId, this.MetadataDescription.SlotIdGenerator(baseline), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public bool IsExpired(DateTime now, long nowUnixTime)
        {
            if (!this.IsCurrentOrNewerSlot(now))
            {
                return true;
            }

            return this.Initialized && this.lastSynchronize > 0 && this.lastSynchronize + MetadataTTL < nowUnixTime;
        }

        internal class QuotaMetadataDescription
        {
            public QuotaMetadataDescription(Func<DateTime, string> slotIdGenerator, TimeSpan cacheTTL)
            {
                this.SlotIdGenerator = slotIdGenerator;
                this.CacheTTL = cacheTTL;
            }

            public Func<DateTime, string> SlotIdGenerator { get; private set; }

            public TimeSpan CacheTTL { get; private set; }
        }
    }
}
