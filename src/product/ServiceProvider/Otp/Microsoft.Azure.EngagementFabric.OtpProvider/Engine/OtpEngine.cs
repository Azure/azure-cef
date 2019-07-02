// <copyright file="OtpEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.Common.Collection;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Common;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Contract;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Helper;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Manager;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Monitor;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Store;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Telemetry;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.WindowsAzure.Storage;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class OtpEngine : IOtpEngine
    {
        /// <summary>
        /// storage wrapper
        /// </summary>
        private readonly IOtpStore otpStore;
        private readonly TelemetryManager otpStorage;

        /// <summary>
        /// Trace message
        /// </summary>
        private readonly Action<string> traceMessage;
        private MetricManager metricManager;

        public OtpEngine(Action<string> traceMessage, IOtpStoreFactory factory, TelemetryManager telemetryManager, MetricManager metricManager)
        {
            this.traceMessage = traceMessage;
            this.otpStore = factory.GetStore();
            this.otpStorage = telemetryManager;
            this.metricManager = metricManager;
        }

        public async Task<ServiceProviderResponse> OtpPushAsync(string account, OtpPushDescription description, ServiceProviderRequest request, string requestId, CancellationToken cancellationToken)
        {
            var channel = OtpChannelHelper.Format(description.Channel);
            var smsProvider = ProviderManager.GetSmsServiceProvider();

            // Check if templete type is 2
            var smsGetRequest = new ServiceProviderRequest
            {
                HttpMethod = "GET",
                Path = "templates/" + description.TemplateName,
                Content = string.Empty,
                Headers = request.Headers,
                QueryNameValuePairs = request.QueryNameValuePairs,
            };
            var subscriptionId = await RequestHelper.GetSubscriptionId(account);
            try
            {
                var result = await smsProvider.OnRequestAsync(smsGetRequest);
                var projson = JObject.Parse(result.Content);
                JToken tpltype;
                if (projson.TryGetValue("tplType", out tpltype) && ((int)tpltype != 2))
                {
                    throw new ArgumentException($"Invalid template type.");
                }

                // generate otpCode
                var code = GetOtpCode((int)description.CodeLength);

                // prepare messageSendRequest for sending request to sms provider
                MessageSendRequest messageSendRequest = new MessageSendRequest()
                {
                    Targets = new List<string>() { description.PhoneNumber },
                    MessageBody = new MessageTemplateBody()
                    {
                        TemplateName = description.TemplateName,
                        TemplateParameters = new PropertyCollection<string>()
                    }
                };
                messageSendRequest.MessageBody.TemplateParameters.Add("otpcode", code);
                var content = JsonConvert.SerializeObject(messageSendRequest);

                // create request for sms provider
                var smsRequest = new ServiceProviderRequest
                {
                    HttpMethod = "POST",
                    Path = "messages",
                    Content = content,
                    Headers = request.Headers,
                    QueryNameValuePairs = request.QueryNameValuePairs
                };

                // send push request to sms provider
                result = await smsProvider.OnRequestAsync(smsRequest);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    return result;
                }

                // Create otp record in db
                var otpcode = await this.otpStore.CreateorUpdateOtpCodeAsync(account, description.PhoneNumber, code, (int)description.ExpireTime);

                // Create otp check history in otp storage table
                await this.otpStorage.CreateOtpCodeHistoryRecord(account, description.PhoneNumber, ActionType.Start.ToString(), DateTime.UtcNow);
                this.metricManager.LogOtpSendSuccess(1, account, subscriptionId, description.Channel);
                OtpProviderEventSource.Current.Info(requestId, this, nameof(this.OtpPushAsync), OperationStates.Succeeded, $"account: {account}, channel: {channel}, phoneNumber: {description.PhoneNumber}");
                return new ServiceProviderResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    JsonContent = new OtpStartOperationResult
                    {
                        ExpireTime = (int)description.ExpireTime
                    }
                };
            }
            catch (Exception ex)
            {
                this.metricManager.LogOtpSendFailed(1, account, subscriptionId, description.Channel);
                while (ex is AggregateException)
                {
                    ex = ex.InnerException;
                }

                OtpProviderEventSource.Current.ErrorException(requestId, this, nameof(this.OtpPushAsync), OperationStates.Failed, $"Failed to send OTP code for account: {account}, channel: {channel}, phoneNumber: {description.PhoneNumber}", ex);

                if ((ex is ArgumentException) || (ex is QuotaExceededException))
                {
                    throw ex;
                }

                throw new Exception(string.Format($"Failed to send OTP code for account: {account}, channel: {channel}, phoneNumber: {description.PhoneNumber}"));
            }
        }

        public async Task<OtpCheckOperationResult> OtpCheckAsync(string account, OtpCheckDescription description, string requestId, CancellationToken cancellationToken)
        {
            OtpCheckOperationResult result = null;
            var subscriptionId = await RequestHelper.GetSubscriptionId(account);
            try
            {
                var otpcode = await this.otpStore.QueryOtpCodeAsync(account, description.PhoneNumber);

                if (otpcode == null)
                {
                    result = new OtpCheckOperationResult(OtpOperationStatus.WRONG_CODE);
                    return result;
                }

                // delete expired code in otp store
                if (otpcode.ExpiredTime < DateTime.UtcNow)
                {
                    await this.otpStore.DeleteOtpCodeAsync(account, description.PhoneNumber);
                    await this.otpStorage.CreateOtpCodeHistoryRecord(account, description.PhoneNumber, ActionType.ExpireDelete.ToString(), DateTime.UtcNow);
                    result = new OtpCheckOperationResult(OtpOperationStatus.CODE_EXPIRED);
                    return result;
                }

                if (otpcode.Code != description.Code)
                {
                    result = new OtpCheckOperationResult(OtpOperationStatus.WRONG_CODE);
                    return result;
                }

                // Create otp check history in otp storage table
                await this.otpStorage.CreateOtpCodeHistoryRecord(account, description.PhoneNumber, ActionType.CheckDelete.ToString(), DateTime.UtcNow);

                // delete code in otp store if check is success
                await this.otpStore.DeleteOtpCodeAsync(account, description.PhoneNumber);
                result = new OtpCheckOperationResult(OtpOperationStatus.SUCCESS);
                this.metricManager.LogOtpCheckSuccess(1, account, subscriptionId, string.Empty);
                OtpProviderEventSource.Current.Info(requestId, this, nameof(this.OtpCheckAsync), OperationStates.Succeeded, $"account: {account}, phoneNumber: {description.PhoneNumber}");

                return result;
            }
            catch (Exception ex)
            {
                this.metricManager.LogOtpCheckFailed(1, account, subscriptionId, string.Empty);
                OtpProviderEventSource.Current.ErrorException(requestId, this, nameof(this.OtpCheckAsync), OperationStates.Failed, $"Failed to check OTP code for account: {account}, phoneNumber: {description.PhoneNumber}", ex);
                throw new Exception(string.Format($"Failed to check OTP code for account: {account}, phoneNumber: {description.PhoneNumber}"));
            }
        }

        public async Task CreateOtpAccountAsync(string account)
        {
            try
            {
                await this.otpStorage.CreateOtpAccountAsync(account);
                OtpProviderEventSource.Current.Info(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.CreateOtpAccountAsync), OperationStates.Succeeded, $"account: {account}");
            }
            catch (Exception ex)
            {
                OtpProviderEventSource.Current.ErrorException(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.CreateOtpAccountAsync), OperationStates.Failed, $"Failed to create OTP information storage table for account: {account}", ex);
                throw new Exception(string.Format($"Failed to create OTP information storage table for account: {account}"));
            }
        }

        public async Task DeleteOtpAccountAsync(string account)
        {
            try
            {
                await this.otpStore.DeleteOtpAccountDataAsync(account);

                // Delete history in Social storage
                await this.otpStorage.DeleteOtpAccount(account);
                OtpProviderEventSource.Current.Info(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteOtpAccountAsync), OperationStates.Succeeded, $"account: {account}");
            }
            catch (Exception ex)
            {
                OtpProviderEventSource.Current.ErrorException(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteOtpAccountAsync), OperationStates.Failed, $"Failed to delete OTP information for account: {account}", ex);
                throw new Exception(string.Format($"Failed to delete OTP information for account: {account}"));
            }
        }

        private string GetOtpCode(int codeLength)
        {
            System.Random rd = new System.Random();
            var min = (ulong)Math.Pow(10, codeLength - 1);
            var max = (ulong)Math.Pow(10, codeLength);
            ulong uRange = (ulong)(max - min);
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                rd.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            }
            while (ulongRand > ulong.MaxValue - (((ulong.MaxValue % uRange) + 1) % uRange));
            var code = (ulong)(ulongRand % uRange) + min;
            return code.ToString();
        }
    }
}