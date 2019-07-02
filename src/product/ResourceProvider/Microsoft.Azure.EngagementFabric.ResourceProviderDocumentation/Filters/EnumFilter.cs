// <copyright file="EnumFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http.Description;
using System.Xml.Linq;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class EnumFilter : ISchemaFilter, IDocumentFilter
    {
        private const string EnumKey = "x-ms-enum";

        private readonly string commentFile;
        private readonly Dictionary<string, Schema> enumSchemas = new Dictionary<string, Schema>();

        public EnumFilter(string commentFile)
        {
            this.commentFile = commentFile;
        }

        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema.properties == null)
            {
                return;
            }

            foreach (var key in schema.properties.Keys.ToList())
            {
                if (schema.properties[key].@enum == null)
                {
                    continue;
                }

                var enumType = PropertyHelper.GetProperty(type, key).PropertyType;

                if (!this.enumSchemas.ContainsKey(enumType.FullName))
                {
                    var enumSchema = schemaRegistry.GetOrRegister(enumType);

                    var enumDescription = new EnumDescription
                    {
                        Name = enumType.Name,
                        ModelAsString = typeof(StringEnumConverter).IsAssignableFrom(enumType.GetCustomAttribute<JsonConverterAttribute>()?.ConverterType)
                    };
                    enumSchema.vendorExtensions.Add(EnumKey, enumDescription);

                    this.enumSchemas.Add(enumType.FullName, enumSchema);
                }

                schema.properties[key] = new Schema
                {
                    @ref = $"#/definitions/{enumType.Name}",
                    description = schema.properties[key].description
                };
            }
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var pair in this.enumSchemas)
            {
                pair.Value.description = this.GetSummary(pair.Key);
                swaggerDoc.definitions.Add(pair.Key.Split('.').Last(), pair.Value);
            }
        }

        private string GetSummary(string fullname)
        {
            try
            {
                var document = XDocument.Load(this.commentFile);
                var members = document.Root.Element("members");
                var rank = members.Elements()
                    .Single(e => e.Attribute("name").Value == $"T:{fullname}");

                return rank.Element("summary").Value.Trim();
            }
            catch
            {
                return null;
            }
        }

        private class EnumDescription
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("modelAsString")]
            public bool ModelAsString { get; set; }
        }
    }
}
