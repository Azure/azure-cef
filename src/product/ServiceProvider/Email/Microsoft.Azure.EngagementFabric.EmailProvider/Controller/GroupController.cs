// <copyright file="GroupController.cs" company="Microsoft Corporation">
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
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;
using EmailConstant = Microsoft.Azure.EngagementFabric.EmailProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Controller
{
    public sealed partial class OperationController
    {
        [HttpPost]
        [Route("groups")]
        public async Task<ServiceProviderResponse> CreateOrUpdateGroupAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromBody] Group request)
        {
            Validator.ArgumentNotNull(request, nameof(request));

            var currentAccount = await EnsureAccount(account, requestId);
            await ValidateDomainExistAsync(currentAccount.EngagementAccount);

            request.EngagementAccount = currentAccount.EngagementAccount;
            var groupResult = await this.engine.CreateOrUpdateGroupAsync(request, requestId);
            if (groupResult.Invalid != null && groupResult.Invalid.Count <= 0)
            {
                groupResult.Invalid = null;
            }

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, groupResult);
        }

        [HttpGet]
        [Route("groups")]
        public async Task<ServiceProviderResponse> ListGroupsAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            Validator.IsTrue<ArgumentException>(count > 0 && count <= EmailConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {EmailConstant.PagingMaxTakeCount}.");

            var currentAccount = await EnsureAccount(account, requestId);

            var token = new DbContinuationToken(continuationToken);
            Validator.IsTrue<ArgumentException>(token.IsValid, nameof(token), "ContinuationToken is invalid.");

            var groups = await this.engine.ListGroupsAsync(currentAccount.EngagementAccount, token, count, requestId);
            var response = ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, groups);
            if (groups.NextLink != null)
            {
                response.Headers = new Dictionary<string, IEnumerable<string>>
                {
                    { ContinuationToken.ContinuationTokenKey, new List<string> { groups.NextLink.Token } }
                };
            }

            return response;
        }

        [HttpGet]
        [Route("groups/{value}")]
        public async Task<ServiceProviderResponse> GetGroupAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            Validator.IsTrue<ArgumentException>(count > 0 && count <= EmailConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {EmailConstant.PagingMaxTakeCount}.");

            var currentAccount = await EnsureAccount(account, requestId);

            var group = await this.engine.GetGroupAsync(currentAccount.EngagementAccount, value, continuationToken, count, requestId);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, group);
        }

        [HttpDelete]
        [Route("groups/{value}")]
        public async Task<ServiceProviderResponse> DeleteGroupAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value)
        {
            var currentAccount = await EnsureAccount(account, requestId);
            await this.engine.DeleteGroupAsync(currentAccount.EngagementAccount, value, requestId);
            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }
    }
}
