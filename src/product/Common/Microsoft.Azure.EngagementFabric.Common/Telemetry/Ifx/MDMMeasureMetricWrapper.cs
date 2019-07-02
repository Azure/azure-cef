// -----------------------------------------------------------------------
// <copyright file="MdmMeasureMetricWrapper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    using System;
    using System.Collections.Generic;
    // using Microsoft.Cloud.InstrumentationFramework;

    using Mock;

    public sealed class MdmMeasureMetricWrapper : IMeasureMetric
    {
        private readonly MeasureMetric wrappedMeasureMetric;
        private readonly string[] dimensionNames;

        public MdmMeasureMetricWrapper(string monitoringAccount, string metricNamespace, string metricName, List<string> dimensionNames, bool addDefaultDimensions)
        {
            this.MetricName = metricName;
            this.dimensionNames = dimensionNames?.ToArray() ?? new string[0];

            var errorContext = default(ErrorContext);
            // this.wrappedMeasureMetric = MeasureMetric.Create(monitoringAccount, metricNamespace, metricName, ref errorContext, addDefaultDimensions, this.dimensionNames);
            // mofied by jin
            this.wrappedMeasureMetric = MeasureMetric.Create(monitoringAccount, metricNamespace, metricName, ref errorContext, addDefaultDimensions, this.dimensionNames);
        }

        public string MetricName { get; private set; }

        public bool LogValue(long value, List<string> measureMetric, ErrorContext errorContext)
        {
            if (measureMetric.Count != this.dimensionNames.Length)
            {
                errorContext.ErrorMessage = $"{measureMetric.Count} dimension values were passed while the metric has {this.dimensionNames.Length} dimensions";
                return false;
            }

            try
            {
                return this.wrappedMeasureMetric.LogValue(value, ref errorContext, measureMetric.ToArray());
            }
            catch (Exception e)
            {
                errorContext.ErrorMessage = e.Message;
                return false;
            }
        }

        public bool LogValue(DateTime timestamp, long value, List<string> measureMetric, ErrorContext errorContext)
        {
            if (measureMetric.Count != this.dimensionNames.Length)
            {
                errorContext.ErrorMessage = $"{measureMetric.Count} dimension values were passed while the metric has {this.dimensionNames.Length} dimensions";
                return false;
            }

            try
            {
                return this.wrappedMeasureMetric.LogValue(timestamp, value, ref errorContext, measureMetric.ToArray());
            }
            catch (Exception e)
            {
                errorContext.ErrorMessage = e.Message;
                return false;
            }
        }
    }
}