// <copyright file="KeyDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The description of the EngagementFabric account key
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class KeyDescription
    {
        /// <summary>
        /// The name of the key
        /// </summary>
        [JsonProperty("name")]
        [ReadOnly(true)]
        public string Name { get; set; }

        /// <summary>
        /// The rank of the key
        /// </summary>
        [JsonProperty("rank")]
        [ReadOnly(true)]
        public KeyRank Rank { get; set; }

        /// <summary>
        /// The value of the key
        /// </summary>
        [JsonProperty("value")]
        [ReadOnly(true)]
        public string Value { get; set; }
    }
}
