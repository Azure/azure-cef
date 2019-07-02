// <copyright file="MetricManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.EmailProvider.Configuration;
using Mock;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    public class MetricManager
    {
        private static readonly List<string> Dimensions = new List<string>
        {
            "Cluster",
            "NodeName",
            "EngagementAccount",
            "SubscriptionId"
        };

        private string cluster;
        private string nodeName;

        private IMeasureMetric sendSuccessCount;
        private IMeasureMetric sendFailedCount;
        private IMeasureMetric deliverSuccessCount;
        private IMeasureMetric deliverFailedCount;

        public MetricManager(ServiceConfiguration configuration)
        {
            Validator.ArgumentNotNullOrEmpty(configuration.MdmAccount, nameof(configuration.MdmAccount));
            Validator.ArgumentNotNullOrEmpty(configuration.MdmMetricNamespace, nameof(configuration.MdmMetricNamespace));

            this.cluster = configuration.Cluster;
            this.nodeName = configuration.NodeName;

            this.sendSuccessCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "SendSuccessCount").CreateMeasureMetric(Dimensions);
            this.sendFailedCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "SendFailedCount").CreateMeasureMetric(Dimensions);
            this.deliverSuccessCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "DeliverSuccessCount").CreateMeasureMetric(Dimensions);
            this.deliverFailedCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "DeliverFailedCount").CreateMeasureMetric(Dimensions);
        }

        public void LogSendSuccess(long count, string engagementAccount, string subscriptionId)
        {
            LogMetric(this.sendSuccessCount, count, engagementAccount, subscriptionId);
        }

        public void LogSendFailed(long count, string engagementAccount, string subscriptionId)
        {
            LogMetric(this.sendFailedCount, count, engagementAccount, subscriptionId);
        }

        public void LogDeliverSuccess(long count, string engagementAccount, string subscriptionId)
        {
            LogMetric(this.deliverSuccessCount, count, engagementAccount, subscriptionId);
        }

        public void LogDeliverFailed(long count, string engagementAccount, string subscriptionId)
        {
            LogMetric(this.deliverFailedCount, count, engagementAccount, subscriptionId);
        }

        private void LogMetric(IMeasureMetric metric, long count, string engagementAccount, string subscriptionId)
        {
            try
            {
                var errorContext = default(ErrorContext);
                var dimensionValues = new List<string>
                {
                    this.cluster,
                    this.nodeName,
                    engagementAccount,
                    subscriptionId
                };

                if (!metric.LogValue(count, dimensionValues, errorContext))
                {
                    EmailProviderEventSource.Current.Warning(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.FailedNotFaulting, $"Logging {metric.MetricName} failed. ErrorMessage={errorContext.ErrorMessage} ErrorCode=0x{errorContext.ErrorCode:X}");
                }
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.Failed, string.Empty, ex);
            }
        }
    }
}
