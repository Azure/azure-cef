// <copyright file="MutabilityAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes
{
    /// <summary>
    /// Indicates the property is mutable
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MutabilityAttribute : Attribute
    {
        public MutabilityAttribute(MutabilityFlags mutability)
        {
            this.Mutability = mutability;
        }

        public MutabilityFlags Mutability { get; private set; }
    }
}
