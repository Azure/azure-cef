// <copyright file="ExampleAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes
{
    /// <summary>
    /// Indicates the examples of the operation
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class ExampleAttribute : Attribute
    {
        public ExampleAttribute(Type exampleType)
        {
            this.ExampleTypes = new[] { exampleType };
        }

        public ExampleAttribute(Type[] exampleTypes)
        {
            this.ExampleTypes = exampleTypes;
        }

        public IEnumerable<Type> ExampleTypes { get; }
    }
}
