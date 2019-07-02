// <copyright file="AzureResourceFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Reflection;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class AzureResourceFilter : ISchemaFilter
    {
        private const string AzureResourceKey = "x-ms-azure-resource";

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type.GetCustomAttribute<AzureResourceAttribute>(false) == null)
            {
                return;
            }

            schema.vendorExtensions.Add(AzureResourceKey, true);
        }
    }
}
