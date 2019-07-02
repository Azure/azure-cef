// -----------------------------------------------------------------------
// <copyright file="MetricsFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    using System.Collections.Generic;

    /// <summary>
    /// Metric Factory which creates metrics which report measurements using the MDM pipeline.
    /// </summary>
    public sealed class MetricsFactory : IMetricsMetricFactory
    {
        private readonly bool addDefaultDimensions;
        private readonly string monitoringAccount;
        private readonly string metricNamespace;
        private readonly string metricName;

        public MetricsFactory(string monitoringAccount, string metricNamespace, string metricName, bool addDefaultDimensions = false)
        {
            this.addDefaultDimensions = addDefaultDimensions;
            this.monitoringAccount = monitoringAccount;
            this.metricNamespace = metricNamespace;
            this.metricName = metricName;
        }

        public IMeasureMetric CreateMeasureMetric()
        {
            List<string> dimensions = new List<string>();
            return this.CreateMeasureMetric(dimensions);
        }

        public IMeasureMetric CreateMeasureMetric(List<string> dimensions)
        {
            return new MdmMeasureMetricWrapper(this.monitoringAccount, this.metricNamespace, this.metricName, dimensions, this.addDefaultDimensions);
        }
    }
}
