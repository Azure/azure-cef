// <copyright file="SkusController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Swashbuckle.Swagger.Annotations;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Controllers
{
    /// <summary>
    /// Controller handling the EngagementFabric available SKUs query
    /// </summary>
    public class SkusController : BaseController
    {
        /// <summary>
        /// List available SKUs of EngagementFabric resource
        /// </summary>
        /// <returns>SKUs</returns>
        [HttpGet]
        [Route("providers/" + NameStore.ProviderNamespace + "/skus")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<SkuDescriptionList> ListAsync()
        {
            return await Task.FromResult(new SkuDescriptionList
            {
                SKUs = SkuStore.Descriptions
            });
        }

        /// <summary>
        /// List available SKUs of EngagementFabric resource
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>SKUs</returns>
        [HttpGet]
        [Route("subscriptions/{subscriptionId}/providers/" + NameStore.ProviderNamespace + "/skus")]
        [SwaggerOperation("SKUs_List")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [NonPageable]
        [Example(typeof(SKUsListExample))]
        public async Task<SkuDescriptionList> ListAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            return await Task.FromResult(new SkuDescriptionList
            {
                SKUs = SkuStore.Descriptions
            });
        }
    }
}