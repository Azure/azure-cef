﻿// <copyright file="ChannelList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The list of the EngagementFabric channels
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class ChannelList
    {
        /// <summary>
        /// EngagementFabric channels
        /// </summary>
        [JsonProperty("value")]
        public IEnumerable<Channel> Channels { get; set; }
    }
}