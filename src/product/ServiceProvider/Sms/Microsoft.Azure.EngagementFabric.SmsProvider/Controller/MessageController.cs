// <copyright file="MessageController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Utils;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;
using SmsConstant = Microsoft.Azure.EngagementFabric.SmsProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Controller
{
    public sealed partial class OperationController
    {
        [HttpPost]
        [Route("messages")]
        public async Task<ServiceProviderResponse> SendMessageAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromBody] MessageSendRequest request)
        {
            Account currentAccount = null;
            try
            {
                SmsProviderEventSource.Current.Info(requestId, this, nameof(this.SendMessageAsync), OperationStates.Received, $"Request is received");

                currentAccount = await this.ValidateAccount(account);
                Validator.ArgumentNotNull(request, nameof(request));
                Validator.ArgumentNotNull(request.MessageBody, nameof(request.MessageBody));
                Validator.ArgumentNotNullOrEmpty(request.MessageBody.TemplateName, request.MessageBody.TemplateName);

                request.Targets = this.ValidatePhoneNumbers(request.Targets);

                var pack = await this.BuildInputMessageAsync(currentAccount, request, requestId);
                var message = pack.InputMessage;
                SmsProviderEventSource.Current.Info(requestId, this, nameof(this.SendMessageAsync), OperationStates.Starting, $"Message is ready. messageId={message.MessageInfo.MessageId}");

                var signatureQuotaName = string.Format(Constant.SmsSignatureMAUNamingTemplate, pack.Signature.Value);
                SmsProviderEventSource.Current.Info(requestId, this, nameof(this.SendMessageAsync), OperationStates.Empty, $"If quota is enabled, request {request.Targets.Count} for account {account}. Otherwise, ignore");
                await QuotaCheckClient.AcquireQuotaAsync(account, SmsConstant.SmsQuotaName, request.Targets.Count);
                await QuotaCheckClient.AcquireQuotaAsync(account, signatureQuotaName, request.Targets.Count);

                try
                {
                    await this.reportManager.OnMessageSentAsync(account, message, pack.Extension);
                    SmsProviderEventSource.Current.Info(requestId, this, nameof(this.SendMessageAsync), OperationStates.Starting, $"Report is updated. messageId={message.MessageInfo.MessageId}");

                    await this.DispatchMessageAsync(message, pack.Extension, requestId);
                    SmsProviderEventSource.Current.Info(requestId, this, nameof(this.SendMessageAsync), OperationStates.Starting, $"Message is dispatched. messageId={message.MessageInfo.MessageId}");

                    this.metricManager.LogSendSuccess(1, account, currentAccount.SubscriptionId ?? string.Empty, pack.Extension.MessageCategory);
                    this.metricManager.LogSendTotal(BillingHelper.GetTotalSegments(message.MessageInfo.MessageBody), account, currentAccount.SubscriptionId ?? string.Empty, pack.Extension.MessageCategory);
                    var response = new MessageSendResponse
                    {
                        MessageId = message.MessageInfo.MessageId,
                        SendTime = message.MessageInfo.SendTime
                    };

                    return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, response);
                }
                catch
                {
                    SmsProviderEventSource.Current.Info(requestId, this, nameof(this.SendMessageAsync), OperationStates.Empty, $"If quota is enabled, release {request.Targets.Count} for account {account}. Otherwise, ignore");
                    await QuotaCheckClient.ReleaseQuotaAsync(account, SmsConstant.SmsQuotaName, request.Targets.Count);
                    await QuotaCheckClient.ReleaseQuotaAsync(account, signatureQuotaName, request.Targets.Count);

                    this.metricManager.LogSendFailed(1, account, currentAccount.SubscriptionId ?? string.Empty, pack.Extension.MessageCategory);
                    throw;
                }
            }
            catch
            {
                this.metricManager.LogSendFailed(1, account, currentAccount?.SubscriptionId ?? string.Empty, null);
                throw;
            }
        }

        [HttpGet]
        [Route("messages/{messageId}")]
        public async Task<ServiceProviderResponse> GetMessageDetailAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string messageId,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            var currentAccount = await this.ValidateAccount(account);
            Validator.IsTrue<ArgumentException>(count > 0 && count <= SmsConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {SmsConstant.PagingMaxTakeCount}.");

            var token = new AzureStorageContinuationToken(continuationToken);
            Validator.IsTrue<ArgumentException>(token.IsValid, nameof(token), "ContinuationToken is invalid.");

            var details = await this.reportManager.GetMessageAsync(account, messageId, count, token.TableContinuationToken);
            Validator.IsTrue<ArgumentException>(details != null, nameof(details), "Message does not exist.");

            var response = ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, details);

            if (details.ContinuationToken != null)
            {
                response.Headers = new Dictionary<string, IEnumerable<string>>
                {
                    { ContinuationToken.ContinuationTokenKey, new List<string> { new AzureStorageContinuationToken(details.ContinuationToken).Token } }
                };
            }

            return response;
        }

        [HttpGet]
        [Route("inbound")]
        public async Task<ServiceProviderResponse> GetInboundMessagesync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account)
        {
            var currentAccount = await this.ValidateAccount(account);
            var messages = await this.inboundManager.GetInboundMessages(account);

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, messages);
        }

        [HttpGet]
        [Route("aggregations/perMessage")]
        public async Task<ServiceProviderResponse> GetPerMessageAggregationAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromQuery("startTime")] long startTimeUnixSeconds = long.MinValue,
            [FromQuery("endTime")] long endTimeUnixSeconds = long.MaxValue,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            var currentAccount = await this.ValidateAccount(account);
            Validator.IsTrue<ArgumentException>(count > 0 && count <= SmsConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {SmsConstant.PagingMaxTakeCount}.");

            var token = new AzureStorageContinuationToken(continuationToken);
            Validator.IsTrue<ArgumentException>(token.IsValid, nameof(token), "ContinuationToken is invalid.");

            var utcNow = DateTime.UtcNow;
            DateTimeOffset startTime, endTime;

            try
            {
                startTime = DateTimeOffset.FromUnixTimeSeconds(startTimeUnixSeconds);
            }
            catch
            {
                startTime = new DateTimeOffset(utcNow.Year, utcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
            }

            try
            {
                endTime = DateTimeOffset.FromUnixTimeSeconds(endTimeUnixSeconds);
            }
            catch
            {
                endTime = new DateTimeOffset(utcNow);
            }

            var result = await this.reportManager.GetPerMessageAggregationAsync(
                account,
                startTime,
                endTime,
                count,
                token.TableContinuationToken);

            var response = ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, result);
            if (result.ContinuationToken != null)
            {
                response.Headers = new Dictionary<string, IEnumerable<string>>
                {
                    {
                        ContinuationToken.ContinuationTokenKey,
                        new[]
                        {
                            new AzureStorageContinuationToken(result.ContinuationToken).Token
                        }
                    }
                };
            }

            return response;
        }

        [HttpGet]
        [Route("aggregations/perPeriod")]
        public async Task<ServiceProviderResponse> GetPerPeriodAggregationAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromQuery("startTime")] long startTime = long.MinValue,
            [FromQuery("endTime")] long endTime = long.MaxValue,
            [FromQuery("resolution")] int seriesResolutionInMinutes = int.MinValue)
        {
            var currentAccount = await this.ValidateAccount(account);

            var utcNow = DateTime.UtcNow;

            try
            {
                DateTimeOffset.FromUnixTimeSeconds(startTime);
            }
            catch
            {
                startTime = new DateTimeOffset(utcNow.Year, utcNow.Month, 1, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds();
            }

            try
            {
                DateTimeOffset.FromUnixTimeSeconds(endTime);
            }
            catch
            {
                endTime = new DateTimeOffset(utcNow).ToUnixTimeSeconds();
            }

            if (seriesResolutionInMinutes < 1)
            {
                seriesResolutionInMinutes = (int)TimeSpan.FromDays(1).TotalMinutes;
            }

            var result = await this.reportManager.GetPerPeriodAggregationAsync(
                requestId,
                account,
                startTime,
                endTime,
                seriesResolutionInMinutes);

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, result);
        }

        private List<string> ValidatePhoneNumbers(List<string> phoneNumbers)
        {
            Validator.IsTrue<ArgumentException>(
                phoneNumbers != null &&
                phoneNumbers.Count > 0,
                nameof(phoneNumbers),
                "Empty phone number");

            Validator.IsTrue<ArgumentException>(
                phoneNumbers != null &&
                phoneNumbers.Count <= SmsConstant.TargetMaxSize,
                nameof(phoneNumbers),
                "Too many phone numbers");

            var filtered = phoneNumbers.Distinct().ToList();
            var invalid = filtered.Where(n => !PhoneNumberValidator.IsNumberValid(n)).ToList();
            Validator.IsTrue<ArgumentException>(
                invalid.Count <= 0,
                nameof(invalid),
                "Phone number formatted incorrectly: {0}",
                string.Join(",", invalid));

            return filtered;
        }

        private void ValidateExtendedCode(string extendedCode)
        {
            Validator.IsTrue<ArgumentException>(
                extendedCode == null || (extendedCode.Length <= SmsConstant.ExtendedCodeCustomLength && !extendedCode.Any(c => !char.IsDigit(c))),
                nameof(extendedCode),
                "Extend code formatted incorrectly.");
        }

        private void ValidateAccountSettings(Account account, Template template)
        {
            // Restrict marketing channel
            var channel = SmsConstant.MessageSendChannelMappings[template.Category];
            if (channel == ChannelType.Marketing && account.AccountSettings != null && account.AccountSettings.PromotionRestricted)
            {
                var localTime = DateTime.UtcNow.ToLocal();
                Validator.IsTrue<ArgumentException>(
                    localTime.Hour >= SmsConstant.PromotionMinHour && localTime.Hour < SmsConstant.PromotionMaxHour,
                    nameof(localTime),
                    "{0} message can only be sent during {1}:00 to {2}:00",
                    template.Category.ToString(),
                    SmsConstant.PromotionMinHour,
                    SmsConstant.PromotionMaxHour);
            }
        }

        private string ValidateTemplateParameters(Template template, Dictionary<string, string> parameters)
        {
            var messageBody = template.Body;

            // Special handle for OTP template
            if (template.Category == MessageCategory.Otp)
            {
                Validator.IsTrue<ArgumentException>(
                    parameters != null && parameters.Count == 1,
                    nameof(parameters),
                    "OTP template requests one parameter.");

                var regex = new Regex(SmsConstant.TemplatePlaceHolderRegex);
                var match = regex.Match(messageBody);
                if (match.Success)
                {
                    messageBody = messageBody.Replace(match.Value, string.Format(SmsConstant.TemplatePlaceHolderFormat, parameters.First().Key));
                }
            }

            if (parameters != null && parameters.Count > 0)
            {
                foreach (var kv in parameters)
                {
                    var key = string.Format(SmsConstant.TemplatePlaceHolderFormat, kv.Key);
                    messageBody = messageBody.Replace(key, kv.Value);
                }
            }

            var length = messageBody.Length;

            Validator.IsTrue<ArgumentException>(
                length >= SmsConstant.SmsBodyMinLength,
                nameof(length),
                "Message is empty");

            Validator.IsTrue<ArgumentException>(
                length <= SmsConstant.SmsBodyMaxLength,
                nameof(length),
                "Message is too long");

            return messageBody;
        }

        private async Task<MessagePack> BuildInputMessageAsync(Account account, MessageSendRequest request, string requestId)
        {
            // Validate extended code
            this.ValidateExtendedCode(request.ExtendedCode);

            // Get template
            var template = await this.store.GetTemplateAsync(account.EngagementAccount, request.MessageBody.TemplateName);
            Validator.IsTrue<ArgumentException>(template != null && template.State == ResourceState.Active, nameof(template), "Template does not exist or is not active");

            // Validate account settings
            this.ValidateAccountSettings(account, template);

            // Get signature
            var signature = await this.store.GetSignatureAsync(account.EngagementAccount, template.Signature);
            Validator.IsTrue<ArgumentException>(signature != null && signature.State == ResourceState.Active, nameof(signature), "Signature does not exist or is not active");

            // Get message body
            var messageBody = this.ValidateTemplateParameters(template, request.MessageBody.TemplateParameters);
            messageBody = string.Format(SmsConstant.SmsBodyFormat, signature.Value, messageBody);

            // Get credential assignement
            var channelType = SmsConstant.MessageSendChannelMappings[template.Category];
            var assignment = await this.credentialManager.GetCredentialAssignmentByAccountAsync(account.EngagementAccount, channelType);
            Validator.IsTrue<ApplicationException>(assignment != null, nameof(assignment), "No active credential assignment for account {0}", account.EngagementAccount);

            // Get credential and metadata
            var credential = await this.credentialManager.GetConnectorCredentialByIdAsync(assignment.ConnectorIdentifier);
            Validator.IsTrue<ApplicationException>(credential != null, nameof(credential), "Invalid credential for account {0}", account.EngagementAccount);

            var metadata = await this.credentialManager.GetMetadata(assignment.ConnectorIdentifier.ConnectorName);

            // Extended code consists of three segments
            var extendedCodes = new List<string>
            {
                assignment.ExtendedCode,
                signature.ExtendedCode,
                request.ExtendedCode
            };

            var extension = new SmsMessageInfoExtension();
            extension.ChannelType = SmsConstant.MessageSendChannelMappings[template.Category];
            extension.MessageCategory = template.Category;
            extension.ExtendedCodes = extendedCodes;

            var message = new InputMessage();
            message.MessageInfo = new MessageInfo();
            message.MessageInfo.EngagementAccount = account.EngagementAccount;
            message.MessageInfo.MessageId = Guid.NewGuid();
            message.MessageInfo.MessageBody = messageBody;
            message.MessageInfo.SendTime = DateTime.UtcNow;
            message.MessageInfo.TrackingId = requestId;
            message.MessageInfo.ExtensionData = extension.ToString();

            message.Targets = request.Targets.AsReadOnly();
            message.ConnectorCredential = credential.ToDataContract(metadata);

            return new MessagePack
            {
                InputMessage = message,
                Extension = extension,
                Signature = signature
            };
        }

        private async Task DispatchMessageAsync(InputMessage message, SmsMessageInfoExtension extension, string requestId)
        {
            // Get partition based on message category
            var partition = this.configuration.DispatchPartitions.SingleOrDefault(p => p.Category == extension.MessageCategory);
            if (partition == null)
            {
                var exception = new ApplicationException($"Cannot dispatch message for category {extension.MessageCategory.ToString()}");
                SmsProviderEventSource.Current.CriticalException(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.DispatchMessageAsync), OperationStates.Failed, "Dispatcher service partition failure", exception);
                throw exception;
            }

            var partionId = this.random.Next(partition.MinPartition, partition.MaxPartition + 1);

            // Config callback service uri
            message.ReportingServiceUri = SmsConstant.ReportingServiceUri;

            // Send to dispatcher service
            var client = this.proxyFactory.CreateServiceProxy<IDispatcherService>(new Uri(SmsConstant.DispatcherServiceUri), new ServicePartitionKey(partionId), TargetReplicaSelector.PrimaryReplica);
            await client.DispatchAsync(new List<InputMessage> { message }, CancellationToken.None);
        }
    }
}
