// <copyright file="DocumentFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    /// <summary>
    /// Apply common document properties
    /// </summary>
    public class DocumentFilter : IDocumentFilter, IOperationFilter
    {
        private const string JsonContentType = "application/json";

        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            ///  updated by jin
            // swaggerDoc.host = "management.azure.com";

            swaggerDoc.produces = new List<string>
            {
                JsonContentType
            };

            swaggerDoc.consumes = new List<string>
            {
                JsonContentType
            };
        }

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (!operation.consumes.Any())
            {
                operation.consumes = null;
            }
            else if (!operation.consumes.Except(new[] { JsonContentType }).Any())
            {
                operation.consumes = null;
            }

            if (!operation.produces.Except(new[] { JsonContentType }).Any())
            {
                operation.produces = null;
            }
        }
    }
}
