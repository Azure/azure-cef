// -----------------------------------------------------------------------
// <copyright file="OperationHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.OtpProvider
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Common;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Configuration;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Contract;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Helper;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
    using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;

    public sealed partial class OtpProvider
    {
        // otp
        private const int DefaultExpireTime = 300;
        private const int DefaultCodeLength = 6;
        private const string DefaultChannel = OtpChannelHelper.Sms;

        private ServiceProviderRequest otpRequest;

        public Task<string> GetProviderName()
        {
            return Task.FromResult(EngagementFabric.Common.Constants.OtpProviderName);
        }

        public async Task OnTenantCreateOrUpdateAsync(Tenant updatedTenant)
        {
            await this.engine.CreateOtpAccountAsync(updatedTenant.AccountName);
        }

        public async Task OnTenantDeleteAsync(string tenantName)
        {
            await this.engine.DeleteOtpAccountAsync(tenantName);
        }

        public async Task<ServiceProviderResponse> OnRequestAsync(ServiceProviderRequest request)
        {
            try
            {
                this.otpRequest = request;
                return await this.dispatcher.DispatchAsync(
                request.HttpMethod,
                request.Path,
                request.Content,
                request.Headers,
                request.QueryNameValuePairs);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ProcessException(this, ex, request.Headers[EngagementFabric.Common.Constants.OperationTrackingIdHeader].FirstOrDefault());
                throw;
            }
        }

        #region OTP

        [HttpPost]
        [Route("start")]
        public async Task<ServiceProviderResponse> StartOtpAsync(
           [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
           [FromHeader] string account,
           [FromBody] OtpPushDescription description)
        {
            Validator.ArgumentNotNull(account, nameof(account));
            Validator.ArgumentNotNull(description, nameof(description));
            Validator.ArgumentNotNullOrEmpty(description.TemplateName, nameof(description.TemplateName));
            Validator.ArgumentNotNullOrEmpty(description.PhoneNumber, nameof(description.PhoneNumber));
            if (description.ExpireTime == null)
            {
                description.ExpireTime = DefaultExpireTime;
            }

            if (description.CodeLength == null)
            {
                description.CodeLength = DefaultCodeLength;
            }

            if (description.Channel == null)
            {
                description.Channel = DefaultChannel;
            }

            if (description.ExpireTime < 60 || description.ExpireTime > 3600)
            {
                throw new ArgumentException($"ExpireTime should be a value between 60 and 3600.");
            }

            if (description.CodeLength < 4 || description.CodeLength > 10)
            {
                throw new ArgumentException($"CodeLength should be a value between 4 and 10.");
            }

            var result = await this.engine.OtpPushAsync(account, description, this.otpRequest, requestId, CancellationToken.None);

            return result;
        }

        [HttpPost]
        [Route("check")]
        public async Task<ServiceProviderResponse> CheckOtpAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromBody] OtpCheckDescription description)
        {
            Validator.ArgumentNotNull(account, nameof(account));
            Validator.ArgumentNotNull(description, nameof(description));
            Validator.ArgumentNotNullOrEmpty(description.PhoneNumber, nameof(description.PhoneNumber));
            Validator.ArgumentNotNullOrEmpty(description.Code, nameof(description.Code));

            var result = await this.engine.OtpCheckAsync(account, description, requestId, CancellationToken.None);
            return new ServiceProviderResponse
            {
                StatusCode = HttpStatusCode.OK,
                JsonContent = result
            };
        }
        #endregion
    }
}
