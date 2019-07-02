// <copyright file="PerPeriodAggregationList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class PerPeriodAggregationList
    {
        public const string TimestampKey = "timestamp";

        public PerPeriodAggregationList(
            IEnumerable<Dictionary<string, object>> values,
            IReadOnlyDictionary<string, object> summary,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes)
        {
            this.Values = values;
            this.Summary = summary;
            this.StartTime = startTime;
            this.EndTime = endTime;
            this.SeriesResolutionInMinutes = seriesResolutionInMinutes;
        }

        [JsonProperty("values")]
        public IEnumerable<Dictionary<string, object>> Values { get; private set; }

        [JsonProperty("summary")]
        public IReadOnlyDictionary<string, object> Summary { get; private set; }

        [JsonProperty("startTime")]
        public long StartTime { get; private set; }

        [JsonProperty("endTime")]
        public long EndTime { get; private set; }

        [JsonProperty("resolution")]
        public int SeriesResolutionInMinutes { get; private set; }
    }
}
