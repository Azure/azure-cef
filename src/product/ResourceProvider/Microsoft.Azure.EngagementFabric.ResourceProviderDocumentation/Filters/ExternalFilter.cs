// <copyright file="ExternalFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Reflection;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class ExternalFilter : ISchemaFilter
    {
        private const string ExternalKey = "x-ms-external";

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type.GetCustomAttribute<ExternalAttribute>() == null)
            {
                return;
            }

            schema.vendorExtensions.Add(ExternalKey, true);
        }
    }
}
