// <copyright file="SubclassFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class SubclassFilter : ISchemaFilter, IDocumentFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (type.BaseType != null && !type.BaseType.Equals(typeof(object)))
            {
                if (schema.allOf == null)
                {
                    schema.allOf = new List<Schema>();
                }

                var parentSchema = schemaRegistry.GetOrRegister(type.BaseType);
                schema.allOf.Add(parentSchema);
            }
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var schema in schemaRegistry.Definitions.Values)
            {
                if (schema.allOf == null)
                {
                    continue;
                }

                var ancestors = new Queue<Schema>(schema.allOf);
                while (ancestors.Any())
                {
                    var ancestor = ancestors.Dequeue();
                    var ancestorTypeName = ancestor.@ref.Split('/').Last();

                    Schema ancestorSchema;
                    if (schemaRegistry.Definitions.TryGetValue(ancestorTypeName, out ancestorSchema))
                    {
                        if (ancestorSchema.properties != null)
                        {
                            foreach (var key in ancestorSchema.properties.Keys)
                            {
                                schema.properties.Remove(key);
                            }
                        }

                        if (ancestorSchema.allOf != null)
                        {
                            foreach (var parent in ancestorSchema.allOf)
                            {
                                ancestors.Enqueue(parent);
                            }
                        }
                    }
                }

                if (!schema.properties.Any())
                {
                    schema.properties = null;
                }
            }
        }
    }
}
