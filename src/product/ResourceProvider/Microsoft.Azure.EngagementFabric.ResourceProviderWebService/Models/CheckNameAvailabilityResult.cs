// <copyright file="CheckNameAvailabilityResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The result of name availability check
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class CheckNameAvailabilityResult
    {
        /// <summary>
        /// The name to be checked
        /// </summary>
        [JsonProperty("nameAvailable")]
        [ReadOnly(true)]
        public bool NameAvailabile { get; set; }

        /// <summary>
        /// The reason if name is unavailable
        /// </summary>
        [JsonProperty("reason")]
        [ReadOnly(true)]
        public CheckNameUnavailableReason Reason { get; set; }

        /// <summary>
        /// The message if name is unavailable
        /// </summary>
        [JsonProperty("message")]
        [ReadOnly(true)]
        public string Message { get; set; }
    }
}