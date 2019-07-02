// <copyright file="ReadOnlyFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Utilities;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class ReadOnlyFilter : ISchemaFilter
    {
        private const string ReadOnlyKey = "readOnly";

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema.properties == null)
            {
                return;
            }

            foreach (var property in schema.properties)
            {
                var attribute = PropertyHelper.GetProperty(type, property.Key)
                    ?.GetCustomAttribute<ReadOnlyAttribute>();

                if (attribute == null || !attribute.IsReadOnly)
                {
                    continue;
                }

                property.Value.vendorExtensions.Add(ReadOnlyKey, true);
            }
        }
    }
}
