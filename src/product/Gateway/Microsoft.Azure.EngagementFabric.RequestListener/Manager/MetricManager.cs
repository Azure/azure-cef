// <copyright file="MetricManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Mock;
// using Microsoft.Cloud.InstrumentationFramework;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Manager
{
    public class MetricManager
    {
        private static readonly List<string> Dimensions = new List<string>
        {
            "Cluster",
            "NodeName",
            "EngagementAccount",
            "SubscriptionId",
            "ServiceProvider"
        };

        private static MetricManager metricManager;

        private string cluster;
        private string nodeName;

        private IMeasureMetric requestSuccessCount;
        private IMeasureMetric requestFailed4xxCount;
        private IMeasureMetric requestFailed5xxCount;
        private IMeasureMetric requestLatency;

        private MetricManager(ServiceConfiguration configuration)
        {
            Validator.ArgumentNotNullOrEmpty(configuration.MdmAccount, nameof(configuration.MdmAccount));
            Validator.ArgumentNotNullOrEmpty(configuration.MdmMetricNamespace, nameof(configuration.MdmMetricNamespace));

            this.cluster = configuration.Cluster;
            this.nodeName = configuration.NodeName;

            this.requestSuccessCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "RequestSuccessCount").CreateMeasureMetric(Dimensions);
            this.requestFailed4xxCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "RequestFailed4xxCount").CreateMeasureMetric(Dimensions);
            this.requestFailed5xxCount = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "RequestFailed5xxCount").CreateMeasureMetric(Dimensions);
            this.requestLatency = new MetricsFactory(configuration.MdmAccount, configuration.MdmMetricNamespace, "RequestLatency").CreateMeasureMetric(Dimensions);
        }

        public static MetricManager Instance
        {
            get
            {
                if (metricManager == null)
                {
                    metricManager = new MetricManager(RequestListenerService.ServiceConfiguration);
                }

                return metricManager;
            }
        }

        public void LogRequestSuccess(long count, string engagementAccount, string subscriptionId, string serviceProvider)
        {
            LogMetric(this.requestSuccessCount, count, engagementAccount, subscriptionId, serviceProvider);
        }

        public void LogRequestFailed4xx(long count, string engagementAccount, string subscriptionId, string serviceProvider)
        {
            LogMetric(this.requestFailed4xxCount, count, engagementAccount, subscriptionId, serviceProvider);
        }

        public void LogRequestFailed5xx(long count, string engagementAccount, string subscriptionId, string serviceProvider)
        {
            LogMetric(this.requestFailed5xxCount, count, engagementAccount, subscriptionId, serviceProvider);
        }

        public void LogRequestLatency(long millionSeconds, string engagementAccount, string subscriptionId, string serviceProvider)
        {
            LogMetric(this.requestLatency, millionSeconds, engagementAccount, subscriptionId, serviceProvider);
        }

        private void LogMetric(IMeasureMetric metric, long count, string engagementAccount, string subscriptionId, string serviceProvider)
        {
            try
            {
                var errorContext = default(ErrorContext);
                var dimensionValues = new List<string>
                {
                    this.cluster,
                    this.nodeName,
                    engagementAccount ?? string.Empty,
                    subscriptionId ?? string.Empty,
                    serviceProvider ?? string.Empty
                };

                if (!metric.LogValue(count, dimensionValues, errorContext))
                {
                    GatewayEventSource.Current.Warning(GatewayEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.FailedNotFaulting, $"Logging {metric.MetricName} failed. ErrorMessage={errorContext.ErrorMessage} ErrorCode=0x{errorContext.ErrorCode:X}");
                }
            }
            catch (Exception ex)
            {
                GatewayEventSource.Current.ErrorException(GatewayEventSource.EmptyTrackingId, this, nameof(this.LogMetric), OperationStates.Failed, string.Empty, ex);
            }
        }
    }
}
