// <copyright file="MessageController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using HtmlAgilityPack;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.DispatcherInterface;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;
using EmailConstant = Microsoft.Azure.EngagementFabric.EmailProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Controller
{
    public sealed partial class OperationController
    {
        [HttpPost]
        [Route("messages")]
        public async Task<ServiceProviderResponse> SendMessageByListAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromBody] MessageSendByListRequest request)
        {
            Account currentAccount = null;
            try
            {
                // TODO: limit on max target?
                Validator.ArgumentNotNull(request, nameof(request));
                Validator.ArgumentNotNull(request.MessageBody, nameof(request.MessageBody));
                Validator.ArgumentNotNullOrEmpty(request.MessageBody.TemplateName, request.MessageBody.TemplateName);
                Validator.IsTrue<ArgumentException>(request.Targets != null && request.Targets.Count > 0, nameof(request.Targets), "Targets is empty");
                Validator.IsTrue<ArgumentException>(request.Targets.Count <= EmailConstant.MailingMaxTargets, nameof(request.Targets), $"Number of Targets could not be more than {EmailConstant.MailingMaxTargets}");

                currentAccount = await EnsureAccount(account, requestId);

                var tuples = await BuildInputMessageAsync(currentAccount, request.MessageBody, request.Targets.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), TargetType.List, requestId);
                var message = tuples.Item1;
                var extension = tuples.Item2;

                // Update report
                await this.reportManager.OnMessageSentAsync(currentAccount.EngagementAccount, message, extension);

                // Dispatch
                await DispatchMessageAsync(message, extension, requestId);

                metricManager.LogSendSuccess(1, currentAccount.EngagementAccount, currentAccount.SubscriptionId ?? string.Empty);

                var response = new MessageSendResponse
                {
                    MessageId = message.MessageInfo.MessageId,
                    SendTime = message.MessageInfo.SendTime
                };

                return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, response);
            }
            catch
            {
                metricManager.LogSendFailed(1, currentAccount.EngagementAccount, currentAccount?.SubscriptionId ?? string.Empty);
                throw;
            }
        }

        [HttpPost]
        [Route("messagesByGroup")]
        public async Task<ServiceProviderResponse> SendMessageByGroupAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromBody] MessageSendByGroupRequest request)
        {
            Account currentAccount = null;
            try
            {
                // TODO: limit on max target?
                Validator.ArgumentNotNull(request, nameof(request));
                Validator.ArgumentNotNull(request.MessageBody, nameof(request.MessageBody));
                Validator.ArgumentNotNullOrEmpty(request.MessageBody.TemplateName, request.MessageBody.TemplateName);
                Validator.IsTrue<ArgumentException>(request.Targets != null && request.Targets.Count > 0, nameof(request.Targets), "Targets is empty");
                Validator.IsTrue<ArgumentException>(request.Targets.Count <= 10, nameof(request.Targets), "Number of Targets could not be more than 10");

                currentAccount = await EnsureAccount(account, requestId);

                var tuples = await BuildInputMessageAsync(currentAccount, request.MessageBody, request.Targets, TargetType.Group, requestId);
                var message = tuples.Item1;
                var extension = tuples.Item2;

                // Update report
                await this.reportManager.OnMessageSentAsync(currentAccount.EngagementAccount, message, extension);

                // Dispatch
                await DispatchMessageAsync(message, extension, requestId);

                metricManager.LogSendSuccess(1, currentAccount.EngagementAccount, currentAccount.SubscriptionId ?? string.Empty);

                var response = new MessageSendResponse
                {
                    MessageId = message.MessageInfo.MessageId,
                    SendTime = message.MessageInfo.SendTime
                };

                return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, response);
            }
            catch
            {
                metricManager.LogSendFailed(1, currentAccount.EngagementAccount, currentAccount?.SubscriptionId ?? string.Empty);
                throw;
            }
        }

        [HttpGet]
        [Route("messages/{messageId}")]
        public async Task<ServiceProviderResponse> GetMessageDetailAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string messageId)
        {
            var currentAccount = await EnsureAccount(account, requestId);
            var details = await this.reportManager.GetReportAsync(currentAccount.EngagementAccount, messageId);
            Validator.IsTrue<ArgumentException>(details != null, nameof(details), "Message does not exist.");

            var response = ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, details);
            return response;
        }

        private string ValidateTemplateParameters(Template template, Dictionary<string, string> parameters)
        {
            var messageBody = template.HtmlMsg;

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var kv in parameters)
                {
                    var key = string.Format(EmailConstant.TemplatePlaceHolderFormat, kv.Key);
                    messageBody = messageBody.Replace(key, kv.Value);
                }
            }

            var length = messageBody.Length;

            Validator.IsTrue<ArgumentException>(
                length >= EmailConstant.EmailBodyMinLength,
                nameof(length),
                "Message is empty");

            Validator.IsTrue<ArgumentException>(
                length <= EmailConstant.EmailBodyMaxLength,
                nameof(length),
                "Message is too long");

            // Validate html format
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(messageBody);
            if (htmlDocument.ParseErrors.Count() > 0)
            {
                throw new ArgumentException(string.Format($"Message body is not valid HTML."));
            }

            return messageBody;
        }

        private async Task<Tuple<InputMessage, EmailMessageInfoExtension>> BuildInputMessageAsync(Account account, MessageTemplateBody messageTemplateBody, List<string> targets, TargetType targetType, string requestId)
        {
            // Get Template
            var template = await this.engine.GetTemplateAsync(account.EngagementAccount, messageTemplateBody.TemplateName, requestId);

            // Get Sender Address
            var sender = await this.engine.GetSenderAsync(template.EngagementAccount, template.SenderId, requestId);

            // Get EmailAccount
            var emailAccount = await this.store.GetEmailAccountAsync(account.EngagementAccount);

            // Get Connector Contract
            var credential = await this.credentialManager.GetConnectorCredentialContractAsync(account.EngagementAccount);

            var extension = new EmailMessageInfoExtension();
            extension.EmailAccount = emailAccount;
            extension.SenderAddress = sender.ToContract();
            extension.TargetType = targetType;
            extension.DisplayName = template.SenderAlias;
            extension.Title = template.Subject;
            extension.EnableUnSubscribe = template.EnableUnSubscribe;

            var message = new InputMessage();
            message.ConnectorCredential = credential;
            message.MessageInfo = new MessageInfo();
            message.MessageInfo.EngagementAccount = account.EngagementAccount;
            message.MessageInfo.MessageId = Guid.NewGuid();
            message.MessageInfo.SendTime = DateTime.UtcNow;
            message.MessageInfo.TrackingId = requestId;
            message.MessageInfo.MessageBody = ValidateTemplateParameters(template, messageTemplateBody.TemplateParameters);
            message.MessageInfo.ExtensionData = extension.ToString();
            message.Targets = targets.AsReadOnly();

            return new Tuple<InputMessage, EmailMessageInfoExtension>(message, extension);
        }

        private async Task DispatchMessageAsync(InputMessage message, EmailMessageInfoExtension extension, string requestId)
        {
            // Get partition based on message category
            var dispatchPartitionCount = this.configuration.DispatchPartitionCount;

            var partionId = this.random.Next(0, dispatchPartitionCount);

            // Config callback service uri
            message.ReportingServiceUri = EmailConstant.ReportingServiceUri;

            // Send to dispatcher service
            var client = this.proxyFactory.CreateServiceProxy<IDispatcherService>(new Uri(EmailConstant.DispatcherServiceUri), new ServicePartitionKey(partionId), TargetReplicaSelector.PrimaryReplica);
            await client.DispatchAsync(new List<InputMessage> { message }, CancellationToken.None);
        }
    }
}
