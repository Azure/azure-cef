// <copyright file="ITimeSeriesManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Mdm
{
    public interface ITimeSeriesManager
    {
        /// <summary>
        /// Try to archive the MDM time series of last month to the archive storage
        /// </summary>
        /// <param name="now">Current time in UNIX seconds</param>
        /// <returns>n/a</returns>
        Task TryArchiveTimeSeriesAsync(long now);

        /// <summary>
        /// Query for time series from both MDM and the archive storage
        /// </summary>
        /// <param name="requestId">Request ID</param>
        /// <param name="dimensionCombination">Dimension names and values</param>
        /// <param name="now">Current time in UNIX seconds</param>
        /// <param name="startTime">Star time in UNIX seconds. Included</param>
        /// <param name="endTime">End time in UNIX seconds. Included</param>
        /// <param name="seriesResolutionInMinutes">Resolution in minutes</param>
        /// <returns>Time series including time stamp and values</returns>
        Task<IReadOnlyDictionary<string, TimeSeries>> GetTimeSeriesAsync(
            string requestId,
            IReadOnlyDictionary<string, string> dimensionCombination,
            long now,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes);
    }
}
