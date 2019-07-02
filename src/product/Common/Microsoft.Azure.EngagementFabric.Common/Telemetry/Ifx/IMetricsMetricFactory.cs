// -----------------------------------------------------------------------
// <copyright file="IMetricsMetricFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    public interface IMetricsMetricFactory
    {
        IMeasureMetric CreateMeasureMetric();

        IMeasureMetric CreateMeasureMetric(List<string> dimensions);
    }
}
