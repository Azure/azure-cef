// <copyright file="SubscriptionProperties.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The properties of the subscription
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class SubscriptionProperties
    {
        /// <summary>
        /// The tenant ID of the subscription
        /// </summary>
        [JsonProperty("tenantId")]
        public string TenantId { get; set; }

        /// <summary>
        /// The placement location ID of the subscription
        /// </summary>
        [JsonProperty("locationPlacementId")]
        public string LocationPlacementId { get; set; }

        /// <summary>
        /// The quota ID of the subscription
        /// </summary>
        [JsonProperty("quotaId")]
        public string QuotaId { get; set; }

        /// <summary>
        /// The registered features of the subscription
        /// </summary>
        [JsonProperty("registeredFeatures")]
        public IEnumerable<SubscriptionFeature> RegisteredFeatures { get; set; }
    }
}
