// <copyright file="TemplateController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.EmailProvider.Utils;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;
using EmailConstant = Microsoft.Azure.EngagementFabric.EmailProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Controller
{
    public sealed partial class OperationController
    {
        [HttpPost]
        [Route("templates")]
        public async Task<ServiceProviderResponse> CreateOrUpdateTemplateAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromBody] TemplateCreateOrUpdateRequest request)
        {
            Validator.ArgumentNotNull(request, nameof(request));
            Validator.ArgumentNotNullOrEmpty(request.Name, nameof(request.Name));
            Validator.ArgumentNotNullOrEmpty(request.SenderAlias, nameof(request.SenderAlias));
            Validator.ArgumentNotNullOrEmpty(request.HtmlMsg, nameof(request.HtmlMsg));
            if (request.SenderAddrID != null)
            {
                Validator.ArgumentValidGuid(request.SenderAddrID, nameof(request.SenderAddrID));
            }

            var currentAccount = await EnsureAccount(account, requestId);

            var template = new Template
            {
                Name = request.Name,
                EngagementAccount = currentAccount.EngagementAccount,
                SenderId = new Guid(request.SenderAddrID),
                SenderAlias = request.SenderAlias,
                Subject = request.Subject,
                HtmlMsg = request.HtmlMsg,
                EnableUnSubscribe = request.EnableUnSubscribe,
                State = ResourceState.Active,
            };

            var templateResult = await this.engine.CreateOrUpdateTemplateAsync(template, requestId);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, templateResult);
        }

        [HttpGet]
        [Route("templates")]
        public async Task<ServiceProviderResponse> ListTemplatesAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            Validator.IsTrue<ArgumentException>(count > 0 && count <= EmailConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {EmailConstant.PagingMaxTakeCount}.");

            var currentAccount = await EnsureAccount(account, requestId);

            var token = new DbContinuationToken(continuationToken);
            Validator.IsTrue<ArgumentException>(token.IsValid, nameof(token), "ContinuationToken is invalid.");

            var templates = await this.engine.ListTemplatesAsync(currentAccount.EngagementAccount, token, count, requestId);
            var response = ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, templates);
            if (templates.NextLink != null)
            {
                response.Headers = new Dictionary<string, IEnumerable<string>>
                {
                    { ContinuationToken.ContinuationTokenKey, new List<string> { templates.NextLink.Token } }
                };
            }

            return response;
        }

        [HttpGet]
        [Route("templates/{value}")]
        public async Task<ServiceProviderResponse> GetTemplateAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            var template = await this.engine.GetTemplateAsync(currentAccount.EngagementAccount, value, requestId);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, template);
        }

        [HttpDelete]
        [Route("templates/{value}")]
        public async Task<ServiceProviderResponse> DeleteTemplateAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            await this.engine.DeleteTemplateAsync(currentAccount.EngagementAccount, value, requestId);
            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }
    }
}
