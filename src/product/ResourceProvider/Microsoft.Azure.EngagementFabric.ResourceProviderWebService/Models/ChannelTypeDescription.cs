// <copyright file="ChannelTypeDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// EngagementFabric channel description
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class ChannelTypeDescription
    {
        /// <summary>
        /// Channel type
        /// </summary>
        [JsonProperty("channelType")]
        public string ChannelType { get; set; }

        /// <summary>
        /// Text description for the channel
        /// </summary>
        [JsonProperty("channelDescription")]
        public string ChannelDescription { get; set; }

        /// <summary>
        /// All the available functions for the channel
        /// </summary>
        [JsonProperty("channelFunctions")]
        public IEnumerable<string> ChannelFunctions { get; set; }
    }
}