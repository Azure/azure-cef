// <copyright file="NonPageableAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes
{
    /// <summary>
    /// Indicates the operation is non-pageable
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NonPageableAttribute : Attribute
    {
        public NonPageableAttribute()
        {
        }
    }
}
