// <copyright file="MetricManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Configuration;
using Mock;
// using Microsoft.Cloud.InstrumentationFramework;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    public class MetricManager
    {
        public const string MetricSendSuccessCount = "SendSuccessCount";
        public const string MetricSendFailedCount = "SendFailedCount";
        public const string MetricSendTotalCount = "SendTotalCount";
        public const string MetricDeliverSuccessCount = "DeliverSuccessCount";
        public const string MetricDeliverFailedCount = "DeliverFailedCount";

        public const string DimensionCluster = "Cluster";
        public const string DimensionNodeName = "NodeName";
        public const string DimensionEngagementAccount = "EngagementAccount";
        public const string DimensionSubscriptionId = "SubscriptionId";
        public const string DimensionMessageCategory = "MessageCategory";

        public static readonly string[] MetricNames = new[]
        {
            MetricSendSuccessCount,
            MetricSendFailedCount,
            MetricSendTotalCount,
            MetricDeliverSuccessCount,
            MetricDeliverFailedCount
        };

        private static readonly List<string> Dimensions = new List<string>
        {
            DimensionCluster,
            DimensionNodeName,
            DimensionEngagementAccount,
            DimensionSubscriptionId,
            DimensionMessageCategory
        };

        private readonly string cluster;
        private readonly string nodeName;

        private readonly IMeasureMetric sendSuccessCount;
        private readonly IMeasureMetric sendFailedCount;
        private readonly IMeasureMetric sendTotalCount;
        private readonly IMeasureMetric deliverSuccessCount;
        private readonly IMeasureMetric deliverFailedCount;

        public MetricManager(ServiceConfiguration configuration)
        {
            Validator.ArgumentNotNullOrEmpty(configuration.MdmAccount, nameof(configuration.MdmAccount));
            Validator.ArgumentNotNullOrEmpty(configuration.MdmMetricNamespace, nameof(configuration.MdmMetricNamespace));

            this.cluster = configuration.Cluster;
            this.nodeName = configuration.NodeName;

            this.sendSuccessCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, MetricSendSuccessCount).CreateMeasureMetric(Dimensions);
            this.sendFailedCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, MetricSendFailedCount).CreateMeasureMetric(Dimensions);
            this.sendTotalCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, MetricSendTotalCount).CreateMeasureMetric(Dimensions);
            this.deliverSuccessCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, MetricDeliverSuccessCount).CreateMeasureMetric(Dimensions);
            this.deliverFailedCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, MetricDeliverFailedCount).CreateMeasureMetric(Dimensions);
        }

        public void LogSendSuccess(long count, string engagementAccount, string subscriptionId, MessageCategory messageCategory)
        {
            this.LogMetric(this.sendSuccessCount, count, engagementAccount, subscriptionId, messageCategory.ToString());
        }

        public void LogSendFailed(long count, string engagementAccount, string subscriptionId, MessageCategory? messageCategory)
        {
            this.LogMetric(this.sendFailedCount, count, engagementAccount, subscriptionId, messageCategory?.ToString() ?? string.Empty);
        }

        public void LogSendTotal(long count, string engagementAccount, string subscriptionId, MessageCategory? messageCategory)
        {
            this.LogMetric(this.sendTotalCount, count, engagementAccount, subscriptionId, messageCategory?.ToString() ?? string.Empty);
        }

        public void LogDeliverSuccess(long count, string engagementAccount, string subscriptionId, MessageCategory messageCategory)
        {
            this.LogMetric(this.deliverSuccessCount, count, engagementAccount, subscriptionId, messageCategory.ToString());
        }

        public void LogDeliverFailed(long count, string engagementAccount, string subscriptionId, MessageCategory? messageCategory)
        {
            this.LogMetric(this.deliverFailedCount, count, engagementAccount, subscriptionId, messageCategory?.ToString() ?? string.Empty);
        }

        private void LogMetric(IMeasureMetric metric, long count, string engagementAccount, string subscriptionId, string messageCategory)
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
                    messageCategory
                };

                if (!metric.LogValue(count, dimensionValues, errorContext))
                {
                    SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.FailedNotFaulting, $"Logging {metric.MetricName} failed. ErrorMessage={errorContext.ErrorMessage} ErrorCode=0x{errorContext.ErrorCode:X}");
                }
            }
            catch (Exception ex)
            {
                SmsProviderEventSource.Current.ErrorException(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.Failed, string.Empty, ex);
            }
        }
    }
}
