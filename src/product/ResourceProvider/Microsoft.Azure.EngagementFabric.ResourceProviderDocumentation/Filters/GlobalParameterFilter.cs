// <copyright file="GlobalParameterFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    /// <summary>
    /// Apply global parameters
    /// </summary>
    public class GlobalParameterFilter : IDocumentFilter, IOperationFilter
    {
        private readonly IReadOnlyDictionary<string, Parameter> globalParameters;
        private readonly IReadOnlyDictionary<string, string> refMapping;

        public GlobalParameterFilter(IReadOnlyDictionary<string, Parameter> globalParameters)
        {
            this.globalParameters = globalParameters;
            this.refMapping = globalParameters.ToDictionary(pair => pair.Value.name, pair => pair.Key);
        }

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            if (swaggerDoc.parameters == null)
            {
                swaggerDoc.parameters = new Dictionary<string, Parameter>();
            }

            foreach (var pair in this.globalParameters)
            {
                swaggerDoc.parameters.Add(pair);
            }
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
            {
                return;
            }

            operation.parameters = operation.parameters.Zip(
                apiDescription.ParameterDescriptions,
                this.TryReplaceByReference).ToList();
        }

        private Parameter TryReplaceByReference(Parameter parameter, ApiParameterDescription description)
        {
            if (parameter.name != description.Name)
            {
                throw new ArgumentException();
            }

            var attribute = description.ParameterDescriptor
                .GetCustomAttributes<GlobalParameterAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return parameter;
            }

            string referenceName;
            if (!this.refMapping.TryGetValue(attribute.GlobalParameterName, out referenceName))
            {
                return parameter;
            }

            return new Parameter
            {
                @ref = $"#/parameters/{referenceName}"
            };
        }
    }
}
