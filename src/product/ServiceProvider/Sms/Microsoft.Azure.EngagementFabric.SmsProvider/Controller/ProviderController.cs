// <copyright file="ProviderController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Controller
{
    using Credential = Model.Credential;

    public sealed partial class OperationController
    {
        [HttpPost]
        [Route("admin/credentials")]
        public async Task<ServiceProviderResponse> CreateOrUpdateCredential(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromBody] Credential request)
        {
            Validator.ArgumentNotNull(request, nameof(request));
            Validator.ArgumentNotNull(request.ConnectorProperties, nameof(request.ConnectorProperties));
            Validator.ArgumentNotNullOrEmpty(request.ConnectorName, nameof(request.ConnectorName));
            Validator.ArgumentNotNullOrEmpty(request.ConnectorKey, nameof(request.ConnectorKey));
            Validator.IsTrue<ArgumentException>(request.ChannelType != ChannelType.Invalid, nameof(request.ChannelType), "Invalid channel type.");

            await this.credentialManager.CreateOrUpdateConnectorCredentialAsync(request.ToConnectorCredential());

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, request);
        }

        [HttpGet]
        [Route("admin/credentials/{provider}/{id}")]
        public async Task<ServiceProviderResponse> GetCredential(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string provider,
            string id)
        {
            var connectorCredential = await this.credentialManager.GetConnectorCredentialByIdAsync(new ConnectorIdentifier(provider, id));
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, new Credential(connectorCredential));
        }

        [HttpDelete]
        [Route("admin/credentials/{provider}/{id}")]
        public async Task<ServiceProviderResponse> DeleteCredential(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string provider,
            string id)
        {
            await this.credentialManager.DeleteConnectorCredentialAsync(new ConnectorIdentifier(provider, id));
            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }
    }
}
