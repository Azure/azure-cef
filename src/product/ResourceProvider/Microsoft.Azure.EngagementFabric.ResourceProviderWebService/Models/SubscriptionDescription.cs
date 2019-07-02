// <copyright file="SubscriptionDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The description of the EngagementFabric subscription
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class SubscriptionDescription
    {
        /// <summary>
        /// The register state of the subscription
        /// </summary>
        [JsonProperty("state")]
        [Required]
        public SubscriptionState? State { get; set; }

        /// <summary>
        /// The registered date of the subscription
        /// </summary>
        [JsonProperty("registrationDate")]
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// The properties of the subscription
        /// </summary>
        [JsonProperty("properties")]
        [Required]
        public SubscriptionProperties Properties { get; set; }
    }
}
