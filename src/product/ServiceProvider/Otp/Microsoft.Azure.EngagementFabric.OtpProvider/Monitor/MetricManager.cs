// <copyright file="MetricManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Monitor
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.Azure.EngagementFabric.OtpProvider;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Configuration;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Contract;
    using Mock;
    // using Microsoft.Cloud.InstrumentationFramework;
    using OtpConstant = Microsoft.Azure.EngagementFabric.OtpProvider.Utils.Constants;

    public class MetricManager
    {
        private static readonly List<string> Dimensions = new List<string>
        {
            "Cluster",
            "NodeName",
            "EngagementAccount",
            "SubscriptionId",
            "ChannelName"
        };

        private string cluster;
        private string nodeName;

        private IMeasureMetric otpSendSuccessCount;
        private IMeasureMetric otpSendFailedCount;
        private IMeasureMetric otpCheckSuccessCount;
        private IMeasureMetric otpCheckFailedCount;

        public MetricManager(ServiceConfiguration configuration)
        {
            Validator.ArgumentNotNullOrEmpty(configuration.MdmAccount, nameof(configuration.MdmAccount));
            Validator.ArgumentNotNullOrEmpty(configuration.MdmMetricNamespace, nameof(configuration.MdmMetricNamespace));

            this.cluster = configuration.Cluster;
            this.nodeName = configuration.NodeName;

            this.otpSendSuccessCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "OtpSendSuccessCount").CreateMeasureMetric(Dimensions);
            this.otpSendFailedCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "OtpSendFailedCount").CreateMeasureMetric(Dimensions);
            this.otpCheckSuccessCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "OtpCheckSuccessCount").CreateMeasureMetric(Dimensions);
            this.otpCheckFailedCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "OtpCheckFailedCount").CreateMeasureMetric(Dimensions);
         }

        public void LogOtpSendSuccess(long count, string engagementAccount, string subscriptionId, string channelName)
        {
            LogMetric(this.otpSendSuccessCount, count, engagementAccount, subscriptionId, channelName.ToString());
        }

        public void LogOtpSendFailed(long count, string engagementAccount, string subscriptionId, string channelName)
        {
            LogMetric(this.otpSendFailedCount, count, engagementAccount, subscriptionId, channelName);
        }

        public void LogOtpCheckSuccess(long count, string engagementAccount, string subscriptionId, string channelName)
        {
            LogMetric(this.otpCheckSuccessCount, count, engagementAccount, subscriptionId, channelName.ToString());
        }

        public void LogOtpCheckFailed(long count, string engagementAccount, string subscriptionId, string channelName)
        {
            LogMetric(this.otpCheckFailedCount, count, engagementAccount, subscriptionId, channelName);
        }

        private void LogMetric(IMeasureMetric metric, long count, string engagementAccount, string subscriptionId, string channelName)
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
                    channelName
                };

                if (!metric.LogValue(count, dimensionValues, errorContext))
                {
                    OtpProviderEventSource.Current.Warning(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.FailedNotFaulting, $"Logging {metric.MetricName} failed. ErrorMessage={errorContext.ErrorMessage} ErrorCode=0x{errorContext.ErrorCode:X}");
                }
            }
            catch (Exception ex)
            {
                OtpProviderEventSource.Current.ErrorException(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.Failed, string.Empty, ex);
            }
        }
    }
}
