// <copyright file="ChannelProperties.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The EngagementFabric channel properties
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class ChannelProperties
    {
        /// <summary>
        /// The channel type
        /// </summary>
        [JsonProperty("channelType")]
        [Required]
        public string ChannelType { get; set; }

        /// <summary>
        /// The functions to be enabled for the channel
        /// </summary>
        [JsonProperty("channelFunctions")]
        public IEnumerable<string> ChannelFunctions { get; set; }

        /// <summary>
        /// The channel credentials
        /// </summary>
        [JsonProperty("credentials")]
        public Dictionary<string, string> Credentials { get; set; }
    }
}