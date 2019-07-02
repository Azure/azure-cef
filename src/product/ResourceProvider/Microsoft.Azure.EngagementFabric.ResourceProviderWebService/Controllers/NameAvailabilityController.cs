// <copyright file="NameAvailabilityController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Controllers
{
    /// <summary>
    /// Controller handling the EngagementFabric name availability check
    /// </summary>
    public class NameAvailabilityController : BaseController
    {
        private readonly IAccountManager accountManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NameAvailabilityController"/> class.
        /// </summary>
        /// <param name="accountManager">The account manager</param>
        public NameAvailabilityController(IAccountManager accountManager)
        {
            this.accountManager = accountManager;
        }

        /// <summary>
        /// Check availability of EngagementFabric resource
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="parameters">Parameter describing the name to be checked</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>Result of the availability check</returns>
        [HttpPost]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.ProviderNamespace + "/checkNameAvailability")]
        [SwaggerOperation("CheckNameAvailability")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(CheckNameAvailabilityExample))]
        public async Task<CheckNameAvailabilityResult> CheckNameAvailabilityAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [FromBody] CheckNameAvailabilityParameter parameters,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(parameters.Type, nameof(parameters.Type));
            Validator.ArgumentNotNullOrrWhiteSpace(parameters.Name, nameof(parameters.Name));

            this.LogActionBegin(
                $"Resource type = {parameters.Type}\n" +
                $"Resource name = {parameters.Name}");

            var result = await this.accountManager.CheckNameAvailabilityAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                parameters.Type,
                parameters.Name);

            this.LogActionEnd(
                $"NameAvailability = {result.NameAvailabile}\n" +
                $"Reason = {result.Reason}\n" +
                $"Message = {result.Message}");
            return result;
        }
    }
}