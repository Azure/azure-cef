// <copyright file="ChannelTypeDescriptionList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// List of the EngagementFabric channel descriptions
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class ChannelTypeDescriptionList
    {
        /// <summary>
        /// Channel descriptions
        /// </summary>
        [JsonProperty("value")]
        public IEnumerable<ChannelTypeDescription> Descriptions { get; set; }
    }
}