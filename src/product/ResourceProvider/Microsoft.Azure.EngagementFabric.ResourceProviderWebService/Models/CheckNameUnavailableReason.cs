// <copyright file="CheckNameUnavailableReason.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The reason of name availability result
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public enum CheckNameUnavailableReason
    {
        /// <summary>
        /// The name is unavailable because the name is invalid
        /// </summary>
        Invalid,

        /// <summary>
        /// The name is unavailable because there is already a resource with the same name
        /// </summary>
        AlreadyExists
    }
}