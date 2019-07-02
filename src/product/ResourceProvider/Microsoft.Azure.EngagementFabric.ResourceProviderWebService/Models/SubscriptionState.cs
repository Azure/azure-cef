// <copyright file="SubscriptionState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The register state of subscription
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubscriptionState
    {
        /// <summary>
        /// Registered
        /// </summary>
        Registered,

        /// <summary>
        /// Unregistered
        /// </summary>
        Unregistered,

        /// <summary>
        /// Warned
        /// </summary>
        Warned,

        /// <summary>
        /// Suspended
        /// </summary>
        Suspended,

        /// <summary>
        /// Deleted
        /// </summary>
        Deleted
    }
}
