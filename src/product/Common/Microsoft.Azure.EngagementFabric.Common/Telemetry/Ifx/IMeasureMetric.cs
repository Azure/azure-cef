// -----------------------------------------------------------------------
// <copyright file="IMeasureMetric.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    
    using System;
    using System.Collections.Generic;
    // using Microsoft.Cloud.InstrumentationFramework;


    using Mock;

    public interface IMeasureMetric
    {
        string MetricName { get; }

        bool LogValue(long value, List<string> dimensionValues, ErrorContext errorContext);

        bool LogValue(DateTime timestamp, long value, List<string> dimensionValues, ErrorContext errorContext);
    }
}