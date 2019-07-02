// <copyright file="TemplateController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;
using SmsConstant = Microsoft.Azure.EngagementFabric.SmsProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Controller
{
    public sealed partial class OperationController
    {
        [HttpPut]
        [Route("admin/accounts/{account}/templates/{value}")]
        public async Task<ServiceProviderResponse> UpdateTemplateAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account,
            string value,
            [FromBody] TemplateUpdateRequest request)
        {
            await ValidateAccount(account);
            Validator.ArgumentNotNull(request, nameof(request));
            Validator.IsTrue<ArgumentException>(request.State != ResourceState.Unknown, nameof(request.State), "Invalid template state.");

            var template = await this.store.GetTemplateAsync(account, value);
            Validator.IsTrue<ArgumentException>(template != null, nameof(template), "Template '{0}' does not exist.", value);

            template.State = request.State;
            template.Message = request.Message;

            template = await this.store.CreateOrUpdateTemplateAsync(template);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, template);
        }

        [HttpPost]
        [Route("templates")]
        public async Task<ServiceProviderResponse> CreateOrUpdateTemplateAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromBody] TemplateCreateOrUpdateRequest request)
        {
            await ValidateAccount(account);
            Validator.ArgumentNotNull(request, nameof(request));
            Validator.ArgumentNotNullOrEmpty(request.Name, request.Name);
            Validator.ArgumentNotNullOrEmpty(request.Signature, request.Signature);
            Validator.IsTrue<ArgumentException>(request.Category != MessageCategory.Invalid, nameof(request.Category), "Invalid message category.");
            ValidateTemplateBody(request.Body, request.Category);

            var signature = await this.store.GetSignatureAsync(account, request.Signature);
            Validator.IsTrue<ResourceNotFoundException>(signature != null, nameof(signature), "The signature '{0}' does not exist.", request.Signature);
            Validator.IsTrue<ArgumentException>(signature.State == ResourceState.Active, nameof(signature), "The signature '{0}' is not active.", request.Signature);
            ValidateSignagureForMessageCategory(signature, request.Category);

            var template = new Template
            {
                EngagementAccount = account,
                Name = request.Name,
                Signature = request.Signature,
                Category = request.Category,
                Body = request.Body,
                State = ResourceState.Pending
            };

            template = await this.store.CreateOrUpdateTemplateAsync(template);

            // Send ops mail
            if (this.mailHelper != null)
            {
                SmsProviderEventSource.Current.Info(requestId, this, nameof(this.CreateOrUpdateTemplateAsync), OperationStates.Starting, $"Sending ops mail. account={account} template={template.Name}");

                var channelType = SmsConstant.MessageSendChannelMappings[template.Category];
                var assignment = await this.credentialManager.GetCredentialAssignmentByAccountAsync(account, channelType);
                this.mailHelper.SendMailOnTemplateCreatedOrUpdated(template, assignment != null ? new CredentialAssignment(assignment) : null, requestId);

                SmsProviderEventSource.Current.Info(requestId, this, nameof(this.CreateOrUpdateTemplateAsync), OperationStates.Succeeded, $"Sent ops mail. account={account} template={template.Name}");
            }
            else
            {
                SmsProviderEventSource.Current.Warning(requestId, this, nameof(this.CreateOrUpdateTemplateAsync), OperationStates.Dropped, $"Cannot send ops email because of mailing is not ready. account={account} template={template.Name}");
            }

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, template);
        }

        [HttpGet]
        [Route("templates")]
        public async Task<ServiceProviderResponse> ListTemplatesAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            await ValidateAccount(account);
            Validator.IsTrue<ArgumentException>(count > 0 && count <= SmsConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {SmsConstant.PagingMaxTakeCount}.");

            var token = new DbContinuationToken(continuationToken);
            Validator.IsTrue<ArgumentException>(token.IsValid, nameof(token), "ContinuationToken is invalid.");

            var templates = await this.store.ListTemplatesAsync(account, token, count);
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
            await ValidateAccount(account);

            var template = await this.store.GetTemplateAsync(account, value);
            Validator.IsTrue<ResourceNotFoundException>(template != null, nameof(template), "The template '{0}' does not exist.", value);

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, template);
        }

        [HttpDelete]
        [Route("templates/{value}")]
        public async Task<ServiceProviderResponse> DeleteTemplateAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value)
        {
            await ValidateAccount(account);

            var template = await this.store.GetTemplateAsync(account, value);
            Validator.IsTrue<ResourceNotFoundException>(template != null, nameof(template), "The template '{0}' does not exist.", value);

            await this.store.DeleteTemplateAsync(account, value);
            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }

        private void ValidateTemplateBody(string body, MessageCategory category)
        {
            var length = body?.Length ?? 0;
            Validator.IsTrue<ArgumentException>(!string.IsNullOrEmpty(body), nameof(body), "Template body cannot be null or empty string.");
            Validator.IsTrue<ArgumentException>(
                length >= SmsConstant.TemplateBodyMinLength && length <= SmsConstant.TemplateBodyMaxLength,
                nameof(body),
                "Invalid template body. Length should be between {0} and {1}",
                SmsConstant.TemplateBodyMinLength,
                SmsConstant.TemplateBodyMaxLength);

            // There can be only one parameter in OTP template
            if (category == MessageCategory.Otp)
            {
                var regex = new Regex(SmsConstant.TemplatePlaceHolderRegex);
                var matches = regex.Matches(body);

                Validator.IsTrue<ArgumentException>(matches.Count == 1, nameof(body), "OTP Template should have one parameter.");
            }
        }

        private void ValidateSignagureForMessageCategory(Signature signature, MessageCategory category)
        {
            if (SmsConstant.MessageAllowedChannelMappings.TryGetValue(category, out List<ChannelType> allowedChannels) &&
                allowedChannels.Contains(signature.ChannelType))
            {
                return;
            }

            throw new ArgumentException($"The signature '{signature.Value}' cannot be used for message category '{category}'");
        }
    }
}
