// -----------------------------------------------------------------------
// <copyright file="OperationHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.SocialProvider
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.Azure.EngagementFabric.SocialConnector;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Common;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Configuration;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Contract;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Helper;
    using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
    using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;

    public sealed partial class SocialProvider
    {
        private const string ChannelIdKey = "id";
        private const string AccessTokenKey = "accessToken";
        private const string PlatformKey = "platform";

        public Task<string> GetProviderName()
        {
            return Task.FromResult(Constants.SocialProviderName);
        }

        public async Task OnTenantCreateOrUpdateAsync(Tenant updatedTenant)
        {
            if (updatedTenant == null)
            {
                return;
            }

            await this.engine.CreateSocialLoginAccountAsync(updatedTenant.AccountName);
        }

        public async Task OnTenantDeleteAsync(string tenantName)
        {
            await this.engine.DeleteSocialLoginAccountAsync(tenantName);
        }

        public async Task<ServiceProviderResponse> OnRequestAsync(ServiceProviderRequest request)
        {
            var trackingId = request.Headers[Constants.OperationTrackingIdHeader].FirstOrDefault();
            try
            {
                SocialProviderEventSource.Current.Info(trackingId, this, nameof(this.OnRequestAsync), OperationStates.Received, request.Path);

                return await this.dispatcher.DispatchAsync(
                request.HttpMethod,
                request.Path,
                request.Content,
                request.Headers,
                request.QueryNameValuePairs);
            }
            catch (SocialChannelExceptionBase ex)
            {
                ExceptionHandler.ProcessException(this, ex, trackingId);
                throw ex.ToGenericException();
            }
            catch (Exception ex)
            {
                ExceptionHandler.ProcessException(this, ex, trackingId);
                throw;
            }
        }

        [HttpPost]
        [Route("userinfo")]
        public async Task<ServiceProviderResponse> CreateOrUpdateUserInfoRecordAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader]string account,
            [FromBody]UserInfoRecordDescription description)
        {
            Validator.ArgumentNotNullOrEmpty(account, nameof(account));
            Validator.ArgumentNotNull(description, nameof(description));
            description.Validate();
            var channel = SocialChannelHelper.Format(description.Channel);
            object channelId = null;
            object platform = null;
            object accessToken = null;
            description.Properties.TryGetValue(ChannelIdKey, out channelId);
            Validator.ArgumentNotNull(channelId, nameof(channelId));
            description.Properties.TryGetValue(AccessTokenKey, out accessToken);
            Validator.ArgumentNotNull(accessToken, nameof(accessToken));
            description.Properties.TryGetValue(PlatformKey, out platform);
            Validator.ArgumentNotNull(platform, nameof(platform));
            var socialPlatform = SocialPlatformHelper.Format(platform.ToString());

            var record = await this.engine.CreateOrUpdateUserInfoRecordAsync(account, channel, accessToken.ToString(), channelId.ToString(), socialPlatform, requestId, CancellationToken.None);
            return new ServiceProviderResponse
            {
                StatusCode = HttpStatusCode.OK,
                JsonContent = record
            };
        }

        [HttpDelete]
        [Route("userinfo")]
        public async Task<ServiceProviderResponse> DeleteUserInfoRecordAsync(
             [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
             [FromHeader]string account,
             [FromQuery]string channel,
             [FromQuery]string id)
        {
            Validator.ArgumentNotNullOrEmpty(account, nameof(account));
            Validator.ArgumentNotNullOrEmpty(channel, nameof(channel));
            Validator.ArgumentNotNullOrEmpty(id, nameof(id));
            channel = SocialChannelHelper.Format(channel);
            await this.engine.DeleteUserInfoRecordAsync(account, channel, id, requestId, CancellationToken.None);
            return new ServiceProviderResponse
            {
                StatusCode = HttpStatusCode.OK
            };
        }
    }
}
