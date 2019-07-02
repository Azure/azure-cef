// <copyright file="ServiceConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Fabric;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;

namespace Microsoft.Azure.EngagementFabric.BillingService.Configuration
{
    public sealed class ServiceConfiguration
    {
        private const string SectionName = "BillingService";
        private readonly ICodePackageActivationContext context;

        public ServiceConfiguration(ICodePackageActivationContext context)
        {
            this.context = context;
            this.StoreAccountConnectionString = this.context.GetConfig<string>(SectionName, "StoreAccountConnectionString");
            this.UsageReportingTableName = this.context.GetConfig<string>(SectionName, "UsageReportingTableName");
            this.UsageReportingQueueName = this.context.GetConfig<string>(SectionName, "UsageReportingQueueName");
            this.PushUsageForWhitelistedSubscriptionsOnly = this.context.GetConfig<bool>(SectionName, "PushUsageForWhitelistedSubscriptionsOnly");

            // Parse WhitelistedSubscriptions, the string should be in format e.g. "xxx;xxx;xxx"
            var subscriptionsString = this.context.GetConfig<string>(SectionName, "WhitelistedSubscriptions");
            var subscriptions = subscriptionsString?.Split(';');
            var subscriptionList = new List<Guid>();
            foreach (var sub in subscriptions)
            {
                if (Guid.TryParse(sub, out Guid subId))
                {
                    subscriptionList.Add(subId);
                }
            }

            this.WhitelistedSubscriptions = subscriptionList.AsReadOnly();

            if (this.PushUsageForWhitelistedSubscriptionsOnly && this.WhitelistedSubscriptions.Count <= 0)
            {
                BillingEventSource.Current.Warning(BillingEventSource.EmptyTrackingId, this, "Construction", OperationStates.NoMatch, $"PushUsageForWhitelistedSubscriptionsOnly is true but no subscription in whitelist.");
            }
        }

        public string StoreAccountConnectionString { get; set; }

        public string UsageReportingTableName { get; set; }

        public string UsageReportingQueueName { get; set; }

        public bool PushUsageForWhitelistedSubscriptionsOnly { get; set; }

        public IReadOnlyList<Guid> WhitelistedSubscriptions { get; set; }
    }
}
