// <copyright file="ProxyOnlyResource.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The base model for the proxy-only Azure resource
    /// </summary>
    [SuppressMessage("Microsoft.StyleCop.CSharp.DocumentationRules", "SA1623:PropertySummaryDocumentationMustMatchAccessors", Justification = "The documentation of model properties MUST NOT start with the phrase 'Gets or sets..', 'Gets..', 'Sets..'")]
    public class ProxyOnlyResource : Resource
    {
    }
}