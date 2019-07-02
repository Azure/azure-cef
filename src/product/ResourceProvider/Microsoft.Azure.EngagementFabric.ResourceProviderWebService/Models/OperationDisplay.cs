// <copyright file="OperationDisplay.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The display information of the EngagementFabric operation
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class OperationDisplay
    {
        /// <summary>
        /// The resource provider namespace of the EngagementFabric operation
        /// </summary>
        [JsonProperty("provider")]
        [ReadOnly(true)]
        public string Provder { get; set; }

        /// <summary>
        /// The resource type of the EngagementFabric operation
        /// </summary>
        [JsonProperty("resource")]
        [ReadOnly(true)]
        public string Resource { get; set; }

        /// <summary>
        /// The name of the EngagementFabric operation
        /// </summary>
        [JsonProperty("operation")]
        [ReadOnly(true)]
        public string Operation { get; set; }

        /// <summary>
        /// The description of the EngagementFabric operation
        /// </summary>
        [JsonProperty("description")]
        [ReadOnly(true)]
        public string Description { get; set; }
    }
}
