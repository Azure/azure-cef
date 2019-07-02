// <copyright file="CloudError.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The default error response
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    [External]
    public class CloudError
    {
        /// <summary>
        /// Content of the error
        /// </summary>
        [JsonProperty("error")]
        public CloudErrorBody Error { get; set; }
    }
}
