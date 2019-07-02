// <copyright file="SkuDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The EngagementFabric SKU description of given resource type
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class SkuDescription
    {
        /// <summary>
        /// The fully qualified resource type
        /// </summary>
        [JsonProperty("resourceType")]
        [ReadOnly(true)]
        public string ResourceType { get; set; }

        /// <summary>
        /// The name of the SKU
        /// </summary>
        [JsonProperty("name")]
        [ReadOnly(true)]
        public string Name { get; set; }

        /// <summary>
        /// The price tier of the SKU
        /// </summary>
        [JsonProperty("tier")]
        [ReadOnly(true)]
        public string Tier { get; set; }

        /// <summary>
        /// The set of locations that the SKU is available
        /// </summary>
        [JsonProperty("locations")]
        [ReadOnly(true)]
        public IEnumerable<string> Locations { get; set; }

        /// <summary>
        /// Locations and zones
        /// </summary>
        [JsonProperty("locationInfo")]
        [ReadOnly(true)]
        public IEnumerable<SkuLocationInfoItem> LocationInfo { get; set; }

        /// <summary>
        /// The restrictions because of which SKU cannot be used
        /// </summary>
        [JsonProperty("restrictions")]
        [ReadOnly(true)]
        public IEnumerable<object> Restrictions { get; set; }
    }
}
