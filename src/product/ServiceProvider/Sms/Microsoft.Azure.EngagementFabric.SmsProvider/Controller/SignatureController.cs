// <copyright file="SignatureController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;
using SmsConstant = Microsoft.Azure.EngagementFabric.SmsProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Controller
{
    public sealed partial class OperationController
    {
        [HttpPost]
        [Route("admin/accounts/{account}/signatures")]
        public async Task<ServiceProviderResponse> CreateOrUpdateSignatureAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account,
            [FromBody] Signature request)
        {
            await ValidateAccount(account);
            Validator.ArgumentNotNull(request, nameof(request));
            Validator.IsTrue<ArgumentException>(request.ChannelType != ChannelType.Invalid, nameof(request.ChannelType), "Invalid channel type.");
            ValidateSignature(request.Value);

            var oldState = ResourceState.Unknown;
            var signature = await this.store.GetSignatureAsync(account, request.Value);
            if (signature == null)
            {
                // Add restriction that do not create an active signature directly
                // Create a signature with pending status first
                // This is to prevent admin providing incorrect name when activate a signature
                Validator.IsTrue<ArgumentException>(request.State != ResourceState.Active, nameof(request.State), "Cannot create a signature with active state.");

                // Create new with a increased code assigned
                signature = new Signature();
                signature.EngagementAccount = account;
                signature.Value = request.Value;
                signature.ExtendedCode = await GetNextExtendedCode(account);
            }

            signature.ChannelType = request.ChannelType;
            signature.State = request.State;
            signature.Message = request.Message;

            signature = await this.store.CreateOrUpdateSignatureAsync(signature);

            // Disable all active templates if signature is not in active state, and vice versa
            if (oldState == ResourceState.Active && signature.State != ResourceState.Active)
            {
                await this.store.UpdateTemplateStateBySignatureAsync(account, signature.Value, ResourceState.Active, ResourceState.Disabled, $"Disabled because signature is in state '{signature.State.ToString()}'");
            }
            else if (oldState != ResourceState.Active && signature.State == ResourceState.Active)
            {
                await this.store.UpdateTemplateStateBySignatureAsync(account, signature.Value, ResourceState.Disabled, ResourceState.Active);
            }

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, new SignatureEx(signature));
        }

        [HttpGet]
        [Route("admin/accounts/{account}/signatures/{value}")]
        public async Task<ServiceProviderResponse> GetSignatureExtensionAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account,
            string value)
        {
            var currentAccount = await ValidateAccount(account);

            var signature = await this.store.GetSignatureAsync(account, value);
            Validator.IsTrue<ResourceNotFoundException>(signature != null, nameof(signature), "The signature '{0}' does not exist.", value);

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, new SignatureEx(signature));
        }

        [HttpPut]
        [Route("admin/accounts/{account}/signatures/{value}/quota")]
        public async Task<ServiceProviderResponse> CreateOrUpdateSignatureQuotaAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account,
            string value,
            [FromBody] int count)
        {
            var currentAccount = await ValidateAccount(account);

            var signature = await this.store.GetSignatureAsync(account, value);
            Validator.IsTrue<ResourceNotFoundException>(signature != null, nameof(signature), "The signature '{0}' does not exist.", value);

            var quotaName = string.Format(Constants.SmsSignatureMAUNamingTemplate, value);
            await QuotaCheckClient.CreateOrUpdateQuotaAsync(account, quotaName, count);

            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }

        [HttpDelete]
        [Route("admin/accounts/{account}/signatures/{value}/quota")]
        public async Task<ServiceProviderResponse> RemoveSignatureQuotaAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account,
            string value)
        {
            var currentAccount = await ValidateAccount(account);

            var signature = await this.store.GetSignatureAsync(account, value);
            Validator.IsTrue<ResourceNotFoundException>(signature != null, nameof(signature), "The signature '{0}' does not exist.", value);

            var quotaName = string.Format(Constants.SmsSignatureMAUNamingTemplate, value);
            await QuotaCheckClient.RemoveQuotaAsync(account, quotaName);

            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [Route("signatures")]
        public async Task<ServiceProviderResponse> ListSignaturesAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            var currentAccount = await ValidateAccount(account);
            Validator.IsTrue<ArgumentException>(count > 0 && count <= SmsConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {SmsConstant.PagingMaxTakeCount}.");

            var token = new DbContinuationToken(continuationToken);
            Validator.IsTrue<ArgumentException>(token.IsValid, nameof(token), "ContinuationToken is invalid.");

            var signatures = await this.store.ListSignaturesAsync(account, token, count);
            var response = ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, signatures);
            if (signatures.NextLink != null)
            {
                response.Headers = new Dictionary<string, IEnumerable<string>>
                {
                    { ContinuationToken.ContinuationTokenKey, new List<string> { signatures.NextLink.Token } }
                };
            }

            return response;
        }

        [HttpGet]
        [Route("signatures/{value}")]
        public async Task<ServiceProviderResponse> GetSignatureAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value)
        {
            var currentAccount = await ValidateAccount(account);

            var signature = await this.store.GetSignatureAsync(account, value);
            Validator.IsTrue<ResourceNotFoundException>(signature != null, nameof(signature), "The signature '{0}' does not exist.", value);

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, signature);
        }

        [HttpDelete]
        [Route("signatures/{value}")]
        public async Task<ServiceProviderResponse> DeleteSignatureAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value)
        {
            var currentAccount = await ValidateAccount(account);

            var signature = await this.store.GetSignatureAsync(account, value);
            Validator.IsTrue<ResourceNotFoundException>(signature != null, nameof(signature), "The signature '{0}' does not exist.", value);

            await this.store.DeleteSignatureAsync(account, value);
            await this.store.DeleteTemplateBySignatureAsync(account, value);

            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }

        private void ValidateSignature(string signature)
        {
            var length = signature?.Length ?? 0;
            Validator.IsTrue<ArgumentException>(!string.IsNullOrEmpty(signature), nameof(signature), "Signature cannot be null or empty string.");
            Validator.IsTrue<ArgumentException>(
                length >= SmsConstant.SignatureMinLength && length <= SmsConstant.SignatureMaxLength,
                nameof(signature),
                "Invalid signature. Length should be between {0} and {1}",
                SmsConstant.SignatureMinLength,
                SmsConstant.SignatureMaxLength);

            var regex = new Regex(@"^[\u4E00-\u9FFFA-Za-z0-9\s]+$");
            Validator.IsTrue<ArgumentException>(regex.IsMatch(signature), nameof(signature), "Signature should contains only Chinese charactor, English charactor and digit.");
            Validator.IsTrue<ArgumentException>(!signature.All(c => char.IsDigit(c)), nameof(signature), "Signature should not contain only digit.");
        }

        private async Task<string> GetNextExtendedCode(string account)
        {
            var signatures = await this.store.ListSignaturesAsync(account, new DbContinuationToken(null), -1);
            if (signatures?.Signatures != null && signatures.Signatures.Count > 0)
            {
                var last = signatures.Signatures.OrderBy(s => s.ExtendedCode).LastOrDefault();
                if (int.TryParse(last.ExtendedCode, out int code) && code < Math.Pow(10, SmsConstant.ExtendedCodeSignatureLength) - 1)
                {
                    return (code + 1).ToString().PadLeft(SmsConstant.ExtendedCodeSignatureLength, '0');
                }
                else
                {
                    throw new ApplicationException($"Failed the generate new code. The last one is '{last.ExtendedCode}'");
                }
            }
            else
            {
                return 1.ToString().PadLeft(SmsConstant.ExtendedCodeSignatureLength, '0');
            }
        }
    }
}
