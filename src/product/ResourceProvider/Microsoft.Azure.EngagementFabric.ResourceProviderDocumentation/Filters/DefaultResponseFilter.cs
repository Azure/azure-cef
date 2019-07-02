// <copyright file="DefaultResponseFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Linq;
using System.Web.Http.Description;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class DefaultResponseFilter : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var attribute = apiDescription.ActionDescriptor
                .GetCustomAttributes<DefaultResponseAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            Response defaultResponse;
            if (!operation.responses.TryGetValue(attribute.StatusCode, out defaultResponse))
            {
                return;
            }

            operation.responses.Remove(attribute.StatusCode);

            defaultResponse.description = "Error response";
            operation.responses.Add("default", defaultResponse);
        }
    }
}
