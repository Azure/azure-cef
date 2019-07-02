// <copyright file="IArchivedTimeSeriesStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Mdm
{
    internal interface IArchivedTimeSeriesStore
    {
        /// <summary>
        /// Archive time series to storage
        /// </summary>
        /// <param name="name">Name of the archive</param>
        /// <param name="series">Time series to be archived</param>
        /// <returns>n/a</returns>
        Task PutTimeSeries(string name, TimeSeries series);

        /// <summary>
        /// Retrieve time series from storage
        /// </summary>
        /// <param name="name">Name of the archive</param>
        /// <returns>Retrieved time series</returns>
        Task<TimeSeries> GetTimeSeries(string name);

        /// <summary>
        /// Check if given time series was already archived
        /// </summary>
        /// <param name="name">Name of the archive</param>
        /// <returns>Returns `true` if the time series was archived</returns>
        Task<bool> IsTimeSeriesExist(string name);

        /// <summary>
        /// Acquire the container lease
        /// </summary>
        /// <returns>Returns `true` if acquired successfully. Otherwise, false</returns>
        Task<bool> AcquireLeaseAsync();

        /// <summary>
        /// Renew the container lease
        /// </summary>
        /// <returns>n/a</returns>
        Task RenewLeaseAsync();

        /// <summary>
        /// Release the container lease
        /// </summary>
        /// <returns>n/a</returns>
        Task ReleaseLeaseAsync();
    }
}
