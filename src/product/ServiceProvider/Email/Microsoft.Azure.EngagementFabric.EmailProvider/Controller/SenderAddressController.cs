// <copyright file="SenderAddressController.cs" company="Microsoft Corporation">
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
        [Route("senderaddr")]
        public async Task<ServiceProviderResponse> CreateOrUpdateSenderAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromBody] Sender request)
        {
            Validator.ArgumentNotNull(request, nameof(request));
            Validator.ArgumentNotNullOrEmpty(request.SenderAddress, nameof(request.SenderAddress));
            Validator.ArgumentNotNullOrEmpty(request.ForwardAddress, nameof(request.ForwardAddress));
            Validator.IsTrue<ArgumentException>(EmailValidator.IsEmailValid(request.SenderAddress), nameof(request.SenderAddress), "SenderAddr is invalid.");
            Validator.IsTrue<ArgumentException>(EmailValidator.IsEmailValid(request.ForwardAddress), nameof(request.ForwardAddress), "ForwardAddr is invalid.");
            if (request.SenderAddrID != null)
            {
                Validator.ArgumentValidGuid(request.SenderAddrID, nameof(request.SenderAddrID));
            }

            var currentAccount = await EnsureAccount(account, requestId);

            request.EngagementAccount = currentAccount.EngagementAccount;
            var senderResult = await this.engine.CreateOrUpdateSenderAsync(request, requestId);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, senderResult);
        }

        [HttpGet]
        [Route("senderaddr")]
        public async Task<ServiceProviderResponse> ListSendersAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            Validator.IsTrue<ArgumentException>(count > 0 && count <= EmailConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {EmailConstant.PagingMaxTakeCount}.");

            var currentAccount = await EnsureAccount(account, requestId);

            var token = new DbContinuationToken(continuationToken);
            Validator.IsTrue<ArgumentException>(token.IsValid, nameof(token), "ContinuationToken is invalid.");

            var senders = await this.engine.ListSendersAsync(currentAccount.EngagementAccount, token, count, requestId);
            var response = ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, senders);
            if (senders.NextLink != null)
            {
                response.Headers = new Dictionary<string, IEnumerable<string>>
                {
                    { ContinuationToken.ContinuationTokenKey, new List<string> { senders.NextLink.Token } }
                };
            }

            return response;
        }

        [HttpGet]
        [Route("senderaddr/{value}")]
        public async Task<ServiceProviderResponse> GetSenderAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            System.Guid value,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            Validator.IsTrue<ArgumentException>(count > 0 && count <= EmailConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {EmailConstant.PagingMaxTakeCount}.");

            var currentAccount = await EnsureAccount(account, requestId);

            var sender = await this.engine.GetSenderAsync(currentAccount.EngagementAccount, value, requestId);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, sender);
        }

        [HttpDelete]
        [Route("senderaddr/{value}")]
        public async Task<ServiceProviderResponse> DeleteSenderAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            Guid value)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            var sender = await this.engine.GetSenderAsync(currentAccount.EngagementAccount, value, requestId);
            await this.engine.DeleteTemplatesbySenderAsync(currentAccount.EngagementAccount, value, requestId);
            await this.engine.DeleteSenderAsync(currentAccount.EngagementAccount, value, requestId);

            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }
    }
}
