// <copyright file="ExternalAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes
{
    /// <summary>
    /// Indicate the model is external
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExternalAttribute : Attribute
    {
    }
}
