// <copyright file="MutabilityFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Utilities;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class MutabilityFilter : ISchemaFilter
    {
        private const string MutabilityKey = "x-ms-mutability";

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema.properties == null)
            {
                return;
            }

            foreach (var property in schema.properties)
            {
                var attribute = PropertyHelper.GetProperty(type, property.Key)
                    ?.GetCustomAttribute<MutabilityAttribute>();

                if (attribute == null)
                {
                    continue;
                }

                var mutabilities = Enum.GetValues(typeof(MutabilityFlags))
                    .Cast<MutabilityFlags>()
                    .Where(f => attribute.Mutability.HasFlag(f))
                    .ToArray();

                property.Value.vendorExtensions.Add(MutabilityKey, mutabilities);
            }
        }
    }
}
