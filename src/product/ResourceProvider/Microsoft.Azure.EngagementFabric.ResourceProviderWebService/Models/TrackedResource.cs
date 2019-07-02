// <copyright file="TrackedResource.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The base model for the tracked Azure resource
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class TrackedResource : Resource
    {
        /// <summary>
        /// The location of the resource
        /// </summary>
        [JsonProperty("location")]
        [Required]
        [Mutability(MutabilityFlags.Read | MutabilityFlags.Create)]
        public string Location { get; set; }

        /// <summary>
        /// The tags of the resource
        /// </summary>
        [JsonProperty("tags")]
        public Dictionary<string, string> Tags { get; set; }

        /// <summary>
        /// The SKU of the resource
        /// </summary>
        [JsonProperty("sku")]
        [Required]
        public SKU SKU { get; set; }
    }
}