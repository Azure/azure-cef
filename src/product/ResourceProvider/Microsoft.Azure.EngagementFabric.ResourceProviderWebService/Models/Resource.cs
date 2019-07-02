// <copyright file="Resource.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The base model for Azure resource
    /// </summary>
    [AzureResource]
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class Resource
    {
        /// <summary>
        /// The ID of the resource
        /// </summary>
        [JsonProperty("id")]
        [ReadOnly(true)]
        public string Id { get; set; }

        /// <summary>
        /// The name of the resource
        /// </summary>
        [JsonProperty("name")]
        [ReadOnly(true)]
        public string Name { get; set; }

        /// <summary>
        /// The fully qualified type of the resource
        /// </summary>
        [JsonProperty("type")]
        [ReadOnly(true)]
        public string Type { get; set; }
    }
}