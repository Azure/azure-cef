// <copyright file="SubscriptionsController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Controllers
{
    /// <summary>
    /// Controller for subscription operations
    /// </summary>
    public class SubscriptionsController : BaseController
    {
        private readonly ISubscriptionManager subscriptionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscriptionsController"/> class.
        /// </summary>
        /// <param name="subscriptionManager">The subscription manager</param>
        public SubscriptionsController(ISubscriptionManager subscriptionManager)
        {
            this.subscriptionManager = subscriptionManager;
        }

        /// <summary>
        /// Create or update subscription registration for EngagementFabric resources
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="model">The parameters describing desired registration state</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>Subscription registration state</returns>
        [HttpPut]
        [Route("subscriptions/{subscriptionId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<SubscriptionDescription> CreateOrUpdateAsync(
            string subscriptionId,
            [FromBody] SubscriptionDescription model,
            [FromQuery("api-version")] string apiVersion)
        {
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNull(model.State, nameof(model.State));
            Validator.ArgumentNotNullOrrWhiteSpace(model.Properties.TenantId, nameof(model.Properties.TenantId));

            this.LogActionBegin(
                $"Subscription ID = {subscriptionId}\n" +
                $"State = {model.State}\n" +
                $"Tenant ID = {model.Properties.TenantId}\n" +
                $"RegistrationDate = {model.RegistrationDate}\n" +
                $"LocationPlacementId = {model.Properties.LocationPlacementId}\n" +
                $"QuotaId = {model.Properties.QuotaId}\n" +
                $"RegisteredFeatures = {JsonConvert.SerializeObject(model.Properties.RegisteredFeatures)}");

            var accounts = await this.subscriptionManager.CreateOrUpdateSubscriptionAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                model);

            this.LogActionEnd(
                $"{accounts.Count()} accounts were updated: {string.Join(", ", accounts)}");
            return model;
        }
    }
}
