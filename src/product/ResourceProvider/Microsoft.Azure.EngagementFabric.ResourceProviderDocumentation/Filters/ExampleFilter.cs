// <copyright file="ExampleFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Web.Http.Description;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.ExampleHelper;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    /// <summary>
    /// Apply example to operations
    /// </summary>
    public class ExampleFilter : IOperationFilter
    {
        private const string ExamplesKey = "x-ms-examples";
        private const string FolderName = "examples";

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var attribute = apiDescription.ActionDescriptor
                .GetCustomAttributes<ExampleAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            var exampleFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FolderName);
            if (!Directory.Exists(exampleFolder))
            {
                Directory.CreateDirectory(exampleFolder);
            }

            operation.vendorExtensions.Add(
                ExamplesKey,
                attribute.ExampleTypes.ToDictionary(
                    t => t.Name,
                    t => ExampleReference.Create(t, exampleFolder)));
        }

        private class ExampleReference
        {
            [JsonProperty("$ref")]
            public string Ref { get; private set; }

            public static ExampleReference Create(Type exampleType, string exampleFolder)
            {
                var example = Activator.CreateInstance(exampleType) as IExample;
                var fileName = $"{exampleType.Name}.json";

                File.WriteAllText(
                    Path.Combine(exampleFolder, fileName),
                    example.Serialize());

                return new ExampleReference
                {
                    Ref = $"./{FolderName}/{fileName}"
                };
            }
        }
    }
}
