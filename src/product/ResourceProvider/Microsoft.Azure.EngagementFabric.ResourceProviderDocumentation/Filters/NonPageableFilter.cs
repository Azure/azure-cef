// <copyright file="NonPageableFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Linq;
using System.Web.Http.Description;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Newtonsoft.Json;
using Swashbuckle.Swagger;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters
{
    public class NonPageableFilter : IOperationFilter
    {
        private const string PageableKey = "x-ms-pageable";

        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var attribute = apiDescription.ActionDescriptor
                .GetCustomAttributes<NonPageableAttribute>()
                .FirstOrDefault();

            if (attribute == null)
            {
                return;
            }

            var nullNextLink = new NextLink
            {
                NextLinkName = null
            };

            operation.vendorExtensions.Add(PageableKey, nullNextLink);
        }

        private class NextLink
        {
            [JsonProperty("nextLinkName", NullValueHandling = NullValueHandling.Include)]
            public string NextLinkName { get; set; }
        }
    }
}
