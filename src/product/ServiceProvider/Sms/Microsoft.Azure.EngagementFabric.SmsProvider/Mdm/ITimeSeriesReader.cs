// <copyright file="ITimeSeriesReader.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Mdm
{
    internal interface ITimeSeriesReader
    {
        string MetricName { get; }

        /// <summary>
        /// List dimension values with metrics in given time span
        /// </summary>
        /// <param name="dimensionNames">List of the dimension names</param>
        /// <param name="startTime">Start time in UNIX seconds. Included</param>
        /// <param name="endTime">End time in UNIX seconds. Included</param>
        /// <returns>Dimension names and values</returns>
        Task<IEnumerable<IReadOnlyDictionary<string, string>>> GetDimensionValuesAsync(
            IEnumerable<string> dimensionNames,
            long startTime,
            long endTime);

        /// <summary>
        /// Get time series for given dimension value and time span
        /// </summary>
        /// <param name="dimensionCombination">Dimension names and values</param>
        /// <param name="startTime">Start time in UNIX seconds. Included</param>
        /// <param name="endTime">End time in UNIX seconds. Included</param>
        /// <param name="seriesResolutionInMinutes">Resolution in minutes</param>
        /// <returns>Time series including time and value</returns>
        Task<TimeSeries> GetTimeSeriesAsync(
            IReadOnlyDictionary<string, string> dimensionCombination,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes);
    }
}
