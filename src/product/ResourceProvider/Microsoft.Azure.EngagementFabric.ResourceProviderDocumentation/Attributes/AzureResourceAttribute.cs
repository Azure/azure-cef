// <copyright file="AzureResourceAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes
{
    /// <summary>
    /// Indicates the model is an ARM resource
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AzureResourceAttribute : Attribute
    {
    }
}
