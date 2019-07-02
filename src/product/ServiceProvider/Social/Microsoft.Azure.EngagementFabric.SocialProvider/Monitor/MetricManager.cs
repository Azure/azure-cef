// <copyright file="MetricManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Monitor
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.Azure.EngagementFabric.SocialProvider;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Configuration;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Contract;
    using Mock;
    // using Microsoft.Cloud.InstrumentationFramework;
    using SocialConstant = Microsoft.Azure.EngagementFabric.SocialProvider.Utils.Constants;

    public class MetricManager
    {
        private static readonly List<string> Dimensions = new List<string>
        {
            "Cluster",
            "NodeName",
            "EngagementAccount",
            "SubscriptionId",
            "ChannelName",
            "Platform"
        };

        private string cluster;
        private string nodeName;

        private IMeasureMetric socialLoginSuccessCount;
        private IMeasureMetric socialLoginFailedCount;

        public MetricManager(ServiceConfiguration configuration)
        {
            Validator.ArgumentNotNullOrEmpty(configuration.MdmAccount, nameof(configuration.MdmAccount));
            Validator.ArgumentNotNullOrEmpty(configuration.MdmMetricNamespace, nameof(configuration.MdmMetricNamespace));

            this.cluster = configuration.Cluster;
            this.nodeName = configuration.NodeName;

            this.socialLoginSuccessCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "SocialLoginSuccessCount").CreateMeasureMetric(Dimensions);
            this.socialLoginFailedCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "SocialLoginFailedCount").CreateMeasureMetric(Dimensions);
        }

        public void LogSocialLoginSuccess(long count, string engagementAccount, string subscriptionId, string channelName, string platform)
        {
            LogMetric(this.socialLoginSuccessCount, count, engagementAccount, subscriptionId, channelName.ToString(), platform.ToString());
        }

        public void LogSocialLoginFailed(long count, string engagementAccount, string subscriptionId, string channelName, string platform)
        {
            LogMetric(this.socialLoginFailedCount, count, engagementAccount, subscriptionId, channelName, platform);
        }

        private void LogMetric(IMeasureMetric metric, long count, string engagementAccount, string subscriptionId, string channelName, string platform)
        {
            try
            {
                var errorContext = default(ErrorContext);
                var dimensionValues = new List<string>
                {
                    this.cluster,
                    this.nodeName,
                    engagementAccount,
                    subscriptionId,
                    channelName,
                    platform
                };
                if (!metric.LogValue(count, dimensionValues, errorContext))
                {
                    SocialProviderEventSource.Current.Warning(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.FailedNotFaulting, $"Logging {metric.MetricName} failed. ErrorMessage={errorContext.ErrorMessage} ErrorCode=0x{errorContext.ErrorCode:X}");
                }
            }
            catch (Exception ex)
            {
                SocialProviderEventSource.Current.ErrorException(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.Failed, string.Empty, ex);
            }
        }
    }
}
