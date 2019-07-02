// <copyright file="ClientFlattenFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class ClientFlattenFilter : ISchemaFilter
    {
        private const string ClientFlattenKey = "x-ms-client-flatten";

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema.properties == null)
            {
                return;
            }

            foreach (var property in schema.properties)
            {
                if (string.Equals(property.Key, "properties", StringComparison.OrdinalIgnoreCase))
                {
                    property.Value.vendorExtensions.Add(ClientFlattenKey, true);
                }
            }
        }
    }
}
