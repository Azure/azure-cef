// <copyright file="ChannelsController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Linq;
using System.Net;
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
    /// Controller for the EngagementFabric channel resource operations
    /// </summary>
    public class ChannelsController : BaseController
    {
        private readonly IAccountManager accountManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelsController"/> class.
        /// </summary>
        /// <param name="accountManager">The account manager</param>
        public ChannelsController(IAccountManager accountManager)
        {
            this.accountManager = accountManager;
        }

        /// <summary>
        /// Create or Update the EngagementFabric channel
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="channelName">The EngagementFabric channel name</param>
        /// <param name="channel">The EngagementFabric channel description</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>The full EngagementFabric channel description</returns>
        [HttpPut]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}/" + NameStore.ChannelResourceType + "/{channelName}")]
        [SwaggerOperation("Channels_CreateOrUpdate")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(ChannelsCreateOrUpdateExample))]
        public async Task<Channel> CreateOrUpdateAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [GlobalParameter("channelName")] string channelName,
            [FromBody] Channel channel,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));
            Validator.ArgumentNotNullOrrWhiteSpace(channelName, nameof(channelName));
            Validator.ArgumentNotNullOrrWhiteSpace(channel.Properties.ChannelType, nameof(channel.Properties.ChannelType));

            this.LogActionBegin($"Credential keys = {string.Join(",", channel.Properties.Credentials.Keys)}");

            var result = await this.accountManager.CreateOrUpdateChannelAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                accountName,
                channelName,
                channel);

            this.LogActionEnd($"Credential keys = {string.Join(",", result.Properties.Credentials.Keys)}");
            return result;
        }

        /// <summary>
        /// Delete the EngagementFabric channel
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="channelName">The EngagementFabric channel name</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>n/a</returns>
        [HttpDelete]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}/" + NameStore.ChannelResourceType + "/{channelName}")]
        [SwaggerOperation("Channels_Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(ChannelsDeleteExample))]
        public async Task<IHttpActionResult> DeleteAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            string channelName,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));
            Validator.ArgumentNotNullOrrWhiteSpace(channelName, nameof(channelName));

            this.LogActionBegin();

            var found = await this.accountManager.DeleteChannelAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                accountName,
                channelName);

            this.LogActionEnd();
            this.LogActionEnd($"Found channel to be deleted: {found}");
            if (found)
            {
                return this.Ok();
            }
            else
            {
                return this.StatusCode(HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Get the EngagementFabric channel
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="channelName">The EngagementFabric channel name</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>The EngagementFabric channel description</returns>
        [HttpGet]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}/" + NameStore.ChannelResourceType + "/{channelName}")]
        [SwaggerOperation("Channels_Get")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(ChannelsGetExample))]
        public async Task<Channel> GetAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [GlobalParameter("channelName")] string channelName,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));
            Validator.ArgumentNotNullOrrWhiteSpace(channelName, nameof(channelName));

            this.LogActionBegin();

            var result = await this.accountManager.GetChannelAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                accountName,
                channelName);

            this.LogActionEnd($"Credential keys = {string.Join(",", result.Properties.Credentials.Keys)}");
            return result;
        }

        /// <summary>
        /// List the EngagementFabric channels
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>List of the EngagementFabric channel descriptions</returns>
        [HttpGet]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}/" + NameStore.ChannelResourceType)]
        [SwaggerOperation("Channels_ListByAccount")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [NonPageable]
        [Example(typeof(ChannelsListExample))]
        public async Task<ChannelList> ListAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));

            this.LogActionBegin();

            var result = new ChannelList
            {
                Channels = await this.accountManager.ListChannelsByAccountAsync(
                    this.Request.GetRequestId(),
                    subscriptionId,
                    resourceGroupName,
                    accountName)
            };

            this.LogActionEnd($"Total {result.Channels.Count()} channels returned");
            return result;
        }
    }
}