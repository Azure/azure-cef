// <copyright file="OperationsController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Swashbuckle.Swagger.Annotations;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Controllers
{
    /// <summary>
    /// Controller for EngagementFabric operations
    /// </summary>
    public class OperationsController : ApiController
    {
        /// <summary>
        /// List operation of EngagementFabric resources
        /// </summary>
        /// <param name="apiVersion">API version</param>
        /// <returns>Operations</returns>
        [HttpGet]
        [Route("providers/" + NameStore.ProviderNamespace + "/operations")]
        [SwaggerOperation("Operations_List")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [NonPageable]
        [Example(typeof(OperationsListExample))]
        public async Task<OperationList> ListAsync(
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);

            return await Task.FromResult(new OperationList
            {
                Operations = OperationStore.Operations
            });
        }
    }
}
