// <copyright file="Operation.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The EngagementFabric operation
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class Operation
    {
        /// <summary>
        /// The name of the EngagementFabric operation
        /// </summary>
        [JsonProperty("name")]
        [ReadOnly(true)]
        public string Name { get; set; }

        /// <summary>
        /// The display content of the EngagementFabric operation
        /// </summary>
        [JsonProperty("display")]
        [ReadOnly(true)]
        public OperationDisplay Display { get; set; }
    }
}
