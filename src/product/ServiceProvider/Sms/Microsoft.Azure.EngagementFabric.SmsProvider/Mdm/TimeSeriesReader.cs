// <copyright file="TimeSeriesReader.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

//using Microsoft.Cloud.Metrics.Client;
//using Microsoft.Cloud.Metrics.Client.Metrics;
//using Microsoft.Online.Metrics.Serialization.Configuration;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Mdm
{
    internal class TimeSeriesReader : ITimeSeriesReader
    {
        //private readonly IMetricReader reader;
        //private readonly MetricIdentifier metricId;

        //public TimeSeriesReader(
        //    IMetricReader reader,
        //    MetricIdentifier metricId)
        //{
        //    this.reader = reader;
        //    this.metricId = metricId;
        //}


        public TimeSeriesReader()
        {
            //this.reader = reader;
            //this.metricId = metricId;
        }

        public string MetricName => "don't know it (jin)";

        public async Task<IEnumerable<IReadOnlyDictionary<string, string>>> GetDimensionValuesAsync(
            IEnumerable<string> dimensionNames,
            long startTime,
            long endTime)
        {
            //var dimensionFilters = dimensionNames.Select(n => DimensionFilter.CreateExcludeFilter(n, string.Empty));

            //var definitions = await this.reader.GetKnownTimeSeriesDefinitionsAsync(
            //    this.metricId,
            //    dimensionFilters,
            //    DateTimeOffset.FromUnixTimeSeconds(startTime).DateTime,
            //    DateTimeOffset.FromUnixTimeSeconds(endTime).DateTime);

            // return definitions.Select(d => d.DimensionCombination.ToDictionary(pair => pair.Key, pair => pair.Value));
            await Task.Delay(0);
            return Enumerable.Empty<IReadOnlyDictionary<string, string>>();
        }

        public async Task<TimeSeries> GetTimeSeriesAsync(
            IReadOnlyDictionary<string, string> dimensionCombination,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes)
        {
            //var definition = new TimeSeriesDefinition<MetricIdentifier>(
            //    this.metricId,
            //    dimensionCombination);

            //var series = await this.reader.GetTimeSeriesAsync(
            //    DateTimeOffset.FromUnixTimeSeconds(startTime).UtcDateTime,
            //    DateTimeOffset.FromUnixTimeSeconds(endTime).UtcDateTime,
            //    SamplingType.Sum,
            //    seriesResolutionInMinutes,
            //    definition);

            //return new TimeSeries
            //{
            //    StartTime = startTime,
            //    EndTime = endTime,
            //    SeriesResolutionInMinutes = seriesResolutionInMinutes,
            //    Datepoints = series.Datapoints
            //        .Where(p => p.Value.HasValue)
            //        .ToDictionary(
            //            p => new DateTimeOffset(p.TimestampUtc, TimeSpan.Zero).ToUnixTimeSeconds(),
            //            p => p.Value.Value),
            //    DimensionCombination = dimensionCombination
            //};

            return await Task.FromResult<TimeSeries>(null);
        }
    }
}
