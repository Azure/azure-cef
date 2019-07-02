// <copyright file="TimeSeriesManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Sms.Common;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Mdm
{
    internal class TimeSeriesManager : ITimeSeriesManager
    {
        private const int ArchiveResolutionInMinutes = 1;

        private readonly IEnumerable<ITimeSeriesReader> readers;
        private readonly IEnumerable<string> dimensionNames;
        private readonly IArchivedTimeSeriesStore metricStore;

        /// <summary>
        /// Return `true` for time series to be archived
        /// </summary>
        private readonly Func<IReadOnlyDictionary<string, string>, bool> dimensionValueSelector;

        public TimeSeriesManager(
            IEnumerable<ITimeSeriesReader> readers,
            IEnumerable<string> dimensionNames,
            IArchivedTimeSeriesStore metricStore,
            Func<IReadOnlyDictionary<string, string>, bool> dimensionValueSelector = null)
        {
            this.readers = readers;
            this.dimensionNames = dimensionNames;
            this.metricStore = metricStore;
            this.dimensionValueSelector = dimensionValueSelector ?? DefaultDimensionValueSelector;
        }

        public async Task TryArchiveTimeSeriesAsync(
            long now)
        {
            // Skip this time of attempt since some other worker is archiving now
            if (!await this.metricStore.AcquireLeaseAsync())
            {
                SmsProviderEventSource.Current.Info(
                    EventSourceBase.EmptyTrackingId,
                    this,
                    nameof(this.TryArchiveTimeSeriesAsync),
                    OperationStates.Locked,
                    $"Skipped archiving metrics due to lease");

                return;
            }

            var startTime = BeginningOfMonth(now);
            foreach (var month in Enumerable.Range(1, 3))
            {
                var endTime = startTime - 60;
                startTime = BeginningOfMonth(endTime);

                foreach (var reader in this.readers)
                {
                    await this.TryArchiveSingleMetricTimeSeriesAsync(
                        reader,
                        startTime,
                        endTime);
                }
            }

            await this.metricStore.ReleaseLeaseAsync();
        }

        public async Task<IReadOnlyDictionary<string, TimeSeries>> GetTimeSeriesAsync(
            string requestId,
            IReadOnlyDictionary<string, string> dimensionCombination,
            long now,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes)
        {
            startTime = FloorToMinute(startTime);
            endTime = FloorToMinute(endTime);

            var mdmBounary = BeginningOfMonth(now);
            var unitsInArchive = (mdmBounary - startTime) / (seriesResolutionInMinutes * 60);

            var output = new Dictionary<string, TimeSeries>();
            foreach (var reader in this.readers)
            {
                TimeSeries series;

                try
                {
                    series = await this.GetSingleTimeSeriesAsync(
                        requestId,
                        reader,
                        dimensionCombination,
                        startTime,
                        endTime,
                        seriesResolutionInMinutes,
                        unitsInArchive);
                }
                catch (Exception ex)
                {
                    SmsProviderEventSource.Current.ErrorException(
                        requestId,
                        this,
                        nameof(this.GetTimeSeriesAsync),
                        OperationStates.Failed,
                        $"Exception raised in retrieving time series {reader.MetricName} for {string.Join("-", dimensionCombination.Values)} ({DateTimeOffset.FromUnixTimeSeconds(startTime)} - {DateTimeOffset.FromUnixTimeSeconds(endTime)})",
                        ex);

                    throw;
                }

                output.Add(
                    reader.MetricName,
                    series);
            }

            return output;
        }

        private static bool DefaultDimensionValueSelector(IReadOnlyDictionary<string, string> dimensionCombination)
        {
#if DEBUG
            return true;
#else
            var hashcode = dimensionCombination.Values
                .Select(s => s.ToLowerInvariant().GetHashCode())
                .Aggregate((h1, h2) => h1 ^ h2);
            var hours = (hashcode % 240) + 120;

            var now = DateTime.UtcNow;
            var startTime = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromHours(hours);
            return now > startTime;
#endif
        }

        private static string ArchiveName(
            string metricName,
            IReadOnlyDictionary<string, string> dimensionCombination,
            long dateTime)
        {
            return $"{string.Join("-", dimensionCombination.Values)}/{metricName}-{DateTimeOffset.FromUnixTimeSeconds(dateTime):yyyy-MM}";
        }

        private static long FloorToMinute(long unixSeconds)
        {
            return unixSeconds / 60 * 60;
        }

        private static long BeginningOfMonth(long unixSeconds)
        {
            var date = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

            return new DateTimeOffset(date.Year, date.Month, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        }

        private static long BeginningOfNextMonth(long unixSeconds)
        {
            var date = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);

            var year = date.Year;
            var month = date.Month + 1;
            if (month > 12)
            {
                month -= 12;
                year++;
            }

            return new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
        }

        private static string GetTimeSeriesDescription(
            string metricName,
            IReadOnlyDictionary<string, string> dimensionCombination,
            long startTime,
            long endTime)
        {
            return $"{metricName}[{string.Join("-", dimensionCombination.Values)}] ({DateTimeOffset.FromUnixTimeSeconds(startTime)} - {DateTimeOffset.FromUnixTimeSeconds(endTime)})";
        }

        private async Task TryArchiveSingleMetricTimeSeriesAsync(
            ITimeSeriesReader reader,
            long startTime,
            long endTime)
        {
            // List dimension values with metrics in last month
            var dimensionCombinations = await reader.GetDimensionValuesAsync(
                this.dimensionNames,
                startTime,
                endTime);

            // Select dimension values to be archived
            var dimensionCombinationsToBeArchived = dimensionCombinations
                .Where(this.dimensionValueSelector)
                .ToList();

            foreach (var dimensionCombination in dimensionCombinationsToBeArchived)
            {
                try
                {
                    // Renew lease for each loop
                    await this.metricStore.RenewLeaseAsync();

                    await this.ArchiveSingleTimeSeriesAsync(
                        reader,
                        dimensionCombination,
                        startTime,
                        endTime);
                }
                catch (Exception ex)
                {
                    SmsProviderEventSource.Current.ErrorException(
                        EventSourceBase.EmptyTrackingId,
                        this,
                        nameof(this.TryArchiveSingleMetricTimeSeriesAsync),
                        OperationStates.Failed,
                        $"Failed to archive metrics {reader.MetricName} of {dimensionCombination}",
                        ex);
                }
            }
        }

        private async Task ArchiveSingleTimeSeriesAsync(
            ITimeSeriesReader reader,
            IReadOnlyDictionary<string, string> dimensionCombination,
            long startTime,
            long endTime)
        {
            // Skip dimension value which has already has been archived
            var archiveName = ArchiveName(
                reader.MetricName,
                dimensionCombination,
                startTime);

            if (await this.metricStore.IsTimeSeriesExist(archiveName))
            {
                return;
            }

            // Get time series from MDM, then archive to storage
            var series = await this.GetTimeSeriesAsync(
                EventSourceBase.EmptyTrackingId,
                reader,
                dimensionCombination,
                startTime,
                endTime,
                ArchiveResolutionInMinutes);

            await this.metricStore.PutTimeSeries(
                archiveName,
                series);

            SmsProviderEventSource.Current.Info(
                EventSourceBase.EmptyTrackingId,
                this,
                nameof(this.ArchiveSingleTimeSeriesAsync),
                OperationStates.Succeeded,
                $"Archived metrics as {archiveName}");
        }

        private async Task<TimeSeries> GetSingleTimeSeriesAsync(
            string requestId,
            ITimeSeriesReader reader,
            IReadOnlyDictionary<string, string> dimensionCombination,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes,
            long unitsInArchive)
        {
            var datapoints = new List<KeyValuePair<long, double>>();
            if (unitsInArchive > 0)
            {
                var archivedEndTime = startTime + (unitsInArchive * seriesResolutionInMinutes * 60);
                if (endTime < archivedEndTime)
                {
                    archivedEndTime = endTime;
                }

                // Load and aggregate the archived time series
                datapoints.AddRange(await this.AggregateArchivedAsync(
                    requestId,
                    reader.MetricName,
                    dimensionCombination,
                    startTime,
                    archivedEndTime,
                    seriesResolutionInMinutes));

                startTime = archivedEndTime;
            }

            if (startTime < endTime)
            {
                // Load rest part from MDM directly
                var series = await this.GetTimeSeriesAsync(
                    requestId,
                    reader,
                    dimensionCombination,
                    startTime,
                    endTime,
                    seriesResolutionInMinutes);
                datapoints.AddRange(series.Datepoints);
            }

            return new TimeSeries
            {
                StartTime = startTime,
                EndTime = endTime,
                SeriesResolutionInMinutes = seriesResolutionInMinutes,
                Datepoints = datapoints.ToDictionary(p => p.Key, p => p.Value),
                DimensionCombination = dimensionCombination
            };
        }

        private async Task<TimeSeries> GetTimeSeriesAsync(
            string requestId,
            ITimeSeriesReader reader,
            IReadOnlyDictionary<string, string> dimensionCombination,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes)
        {
            var timeSeriesDescription = GetTimeSeriesDescription(
                reader.MetricName,
                dimensionCombination,
                startTime,
                endTime);

            SmsProviderEventSource.Current.Info(
                requestId,
                this,
                nameof(this.GetTimeSeriesAsync),
                OperationStates.Starting,
                $"Start loading MDM metric {timeSeriesDescription}");

            var output = await reader.GetTimeSeriesAsync(
                dimensionCombination,
                startTime,
                endTime,
                seriesResolutionInMinutes);

            SmsProviderEventSource.Current.Info(
                requestId,
                this,
                nameof(this.GetTimeSeriesAsync),
                OperationStates.Succeeded,
                $"Succeeded in loading MDM metric {timeSeriesDescription}. {output.Datepoints.Count} datapoints returned");

            return output;
        }

        private async Task<IEnumerable<KeyValuePair<long, double>>> AggregateArchivedAsync(
            string requestId,
            string metricName,
            IReadOnlyDictionary<string, string> dimensionCombination,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes)
        {
            var fullTimeSeriesDescription = GetTimeSeriesDescription(
                metricName,
                dimensionCombination,
                startTime,
                endTime);

            SmsProviderEventSource.Current.Info(
                requestId,
                this,
                nameof(this.GetTimeSeriesAsync),
                OperationStates.Starting,
                $"Start aggregating time series {fullTimeSeriesDescription}");

            var aggregator = new TimeSeriesAggregator(
                startTime,
                seriesResolutionInMinutes);
            var datapoints = new List<KeyValuePair<long, double>>();

            for (var beginningOfMonth = BeginningOfMonth(startTime); beginningOfMonth <= endTime; beginningOfMonth = BeginningOfNextMonth(beginningOfMonth))
            {
                var timeSeriesDescription = GetTimeSeriesDescription(
                    metricName,
                    dimensionCombination,
                    beginningOfMonth,
                    endTime);

                SmsProviderEventSource.Current.Info(
                    requestId,
                    this,
                    nameof(this.AggregateArchivedAsync),
                    OperationStates.Starting,
                    $"Start loading archived time series {timeSeriesDescription}");

                var archiveName = ArchiveName(
                    metricName,
                    dimensionCombination,
                    beginningOfMonth);

                var archivedSeries = await this.metricStore.GetTimeSeries(archiveName);
                if (archivedSeries == null)
                {
                    SmsProviderEventSource.Current.Info(
                        requestId,
                        this,
                        nameof(this.AggregateArchivedAsync),
                        OperationStates.Dropped,
                        $"Archived time series {timeSeriesDescription} skipped: no archive file or unexpected schema");

                    continue;
                }
                else
                {
                    SmsProviderEventSource.Current.Info(
                        requestId,
                        this,
                        nameof(this.AggregateArchivedAsync),
                        OperationStates.Succeeded,
                        $"Succeeded in loading time series {timeSeriesDescription}. {archivedSeries.Datepoints.Count} datapoints returned");
                }

                foreach (var pair in archivedSeries.Datepoints.Where(pair => pair.Key >= startTime && pair.Key <= endTime))
                {
                    var output = aggregator.Add(pair.Key, pair.Value);
                    if (output.HasValue)
                    {
                        datapoints.Add(output.Value);
                    }
                }
            }

            if (aggregator.Sum.HasValue)
            {
                datapoints.Add(new KeyValuePair<long, double>(aggregator.StartTime, aggregator.Sum.Value));
            }

            SmsProviderEventSource.Current.Info(
                requestId,
                this,
                nameof(this.AggregateArchivedAsync),
                OperationStates.Succeeded,
                $"Succeeded in Aggregating time series {fullTimeSeriesDescription}. {datapoints.Count} datapoints generated");

            return datapoints;
        }
    }
}
