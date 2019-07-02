// <copyright file="TimeSeries.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Mdm
{
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class TimeSeries
    {
        /// <summary>
        /// The start time in UNIX seconds
        /// It may be earlier than the time stamp of first data point, in case corresponding value is `null`
        /// </summary>
        public long StartTime { get; set; }

        /// <summary>
        /// The end time in UNIX seconds
        /// It may be later than the time stamp of last data point, in case corresponding value is `null`
        /// </summary>
        public long EndTime { get; set; }

        /// <summary>
        /// Resolution in minutes
        /// </summary>
        public int SeriesResolutionInMinutes { get; set; }

        /// <summary>
        /// Dimension combination
        /// </summary>
        public IReadOnlyDictionary<string, string> DimensionCombination { get; set; }

        /// <summary>
        /// Time stamp and values
        /// </summary>
        public IReadOnlyDictionary<long, double> Datepoints { get; set; }
    }
}
