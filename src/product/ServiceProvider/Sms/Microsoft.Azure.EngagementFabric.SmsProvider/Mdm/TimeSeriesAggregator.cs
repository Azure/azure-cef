// <copyright file="TimeSeriesAggregator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Mdm
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    internal class TimeSeriesAggregator
    {
        private readonly long resolution;

        public TimeSeriesAggregator(
            long startTime,
            int seriesResolutionInMinutes)
        {
            this.StartTime = startTime;
            this.resolution = seriesResolutionInMinutes * 60;
        }

        /// <summary>
        /// Current sum of current slot
        /// </summary>
        public double? Sum { get; private set; } = null;

        /// <summary>
        /// Start time of current slot in UNIX seconds
        /// </summary>
        public long StartTime { get; private set; }

        /// <summary>
        /// Add new value
        /// </summary>
        /// <param name="unixSeconds">Time stamp</param>
        /// <param name="value">Data value</param>
        /// <returns>Aggregated result in slot completed. Otherwise, null</returns>
        public KeyValuePair<long, double>? Add(long unixSeconds, double value)
        {
            if (this.StartTime == long.MinValue)
            {
                this.StartTime = unixSeconds;
            }

            KeyValuePair<long, double>? output = null;
            while (unixSeconds >= this.StartTime + this.resolution)
            {
                if (this.Sum.HasValue)
                {
                    output = new KeyValuePair<long, double>(this.StartTime, this.Sum.Value);
                    this.Sum = null;
                }

                this.StartTime += this.resolution;
            }

            this.Sum = this.Sum.GetValueOrDefault() + value;
            return output;
        }
    }
}
