// <copyright file="SocialEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.Common.Collection;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.Azure.EngagementFabric.SocialConnector;
    using Microsoft.Azure.EngagementFabric.SocialProvider;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Common;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Contract;
    using Microsoft.Azure.EngagementFabric.SocialProvider.EntityFramework;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Helper;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Monitor;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Store;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Telemetry;
    using Microsoft.Azure.EngagementFabric.TenantCache;

    public class SocialEngine : ISocialEngine
    {
        // Social
        private const string ChannelIdKey = "id";
        private const string AccessTokenKey = "accessToken";
        private const string PlatformKey = "platform";
        private const string AppIdKey = "appId";
        private const string SocialProfileKey = "socialProfile";

        private static readonly Dictionary<string, ConnectorCreator> ConnectorCreators =
        new Dictionary<string, ConnectorCreator>(StringComparer.InvariantCultureIgnoreCase)
        {
                // Create social connectors
        };

        private static readonly ReadOnlyTenantCacheClient TenantCacheClient = ReadOnlyTenantCacheClient.GetClient(true);

        /// <summary>
        /// SQL storage wrapper
        /// </summary>
        private readonly TelemetryManager telemetryManager;

        private readonly Action<string> traceMessage;
        private StoreManager storeManager;
        private MetricManager metricManager;

        public SocialEngine(Action<string> traceMessage, string connectionString, MetricManager metricManager)
        {
            this.traceMessage = traceMessage;
            this.storeManager = new StoreManager(connectionString);
            this.telemetryManager = new TelemetryManager();
            this.metricManager = metricManager;
        }

        private delegate ISocialConnector ConnectorCreator();

        public async Task<UserInfoRecord> CreateOrUpdateUserInfoRecordAsync(string account, string channel, string accessToken, string channelId, string socialPlatform, string requestId, CancellationToken cancellationToken)
        {
            UserInfoRecordDescription profileDescription = new UserInfoRecordDescription(channel);
            UserInfoRecordDescription description = new UserInfoRecordDescription(channel);
            description.Properties.Add(ChannelIdKey, channelId);
            description.Properties.Add(AccessTokenKey, accessToken);
            description.Properties.Add(PlatformKey, socialPlatform);
            PropertyCollection<object> profile = new PropertyCollection<object>();

            var subscriptionId = await RequestHelper.GetSubscriptionId(account);
            UserInfo userInfo = new UserInfo();
            bool updatedQuotaTotal = false;
            bool updatedQuotaMAU = false;
            try
            {
                // get appid for QQ and add into description
                if (channel == SocialChannelHelper.QQ)
                {
                    var appId = await this.GetCredentialAsync(account, channel, socialPlatform);
                    Validator.IsTrue<ArgumentException>(appId != null, nameof(appId), "No app_id for getting user information from {0}", description.Channel);
                    description.Properties.Add(AppIdKey, appId);
                }

                // GET Profile from Social service with connector
                var connector = this.CreateConnector(channel);
                profile.Add(ChannelIdKey, channelId);
                var socialprofile = await connector.GetSocialProfileAsync(description.Properties);
                profile.Add(SocialProfileKey, socialprofile);
                profileDescription.Properties = profile;

                var storeAgent = await storeManager.GetStoreAgent();
                userInfo = await storeAgent.UserInfoStore.GetUserInfoAsync(account, profileDescription.Channel, channelId.ToString());

                // Create or update profile in Social store
                var userInfoResult = await storeAgent.UserInfoStore.CreateorUpdateUserInfoAsync(account, channelId.ToString(), profileDescription);

                // Update history in Social storage
                await this.telemetryManager.CreateSocialLoginHistoryRecordAsync(storeAgent, account, channel, channelId, socialPlatform, userInfoResult.Action.ToString(), userInfoResult.UserInfo.ModifiedTime);

                // Update Social Login MAU and Total quota for New user
                if (userInfo == null)
                {
                    await QuotaCheckClient.AcquireQuotaAsync(account, Constants.SocialLoginTotal, 1);
                    updatedQuotaTotal = true;
                    await QuotaCheckClient.AcquireQuotaAsync(account, Constants.SocialLoginMAU, 1);
                    updatedQuotaMAU = true;
                }
                else
                {
                    // Update Social Login MAU quota for login first time this month
                    if (userInfo.ModifiedTime < new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1))
                    {
                        await QuotaCheckClient.AcquireQuotaAsync(account, Constants.SocialLoginMAU, 1);
                        updatedQuotaMAU = true;
                    }
                }

                var userInfoRecord = userInfoResult.UserInfo.ToUserInfoRecord();
                this.metricManager.LogSocialLoginSuccess(1, account, subscriptionId ?? string.Empty, channel, socialPlatform);
                SocialProviderEventSource.Current.Info(requestId, this, nameof(this.CreateOrUpdateUserInfoRecordAsync), OperationStates.Succeeded, $"account: {account}, channel: {channel}, platform: {socialPlatform}, channelId: {channelId}");
                return userInfoRecord;
            }
            catch (Exception ex)
            {
                if (updatedQuotaTotal == true)
                {
                    await QuotaCheckClient.ReleaseQuotaAsync(account, Constants.SocialLoginTotal, 1);
                }

                if (updatedQuotaMAU == true)
                {
                    await QuotaCheckClient.ReleaseQuotaAsync(account, Constants.SocialLoginMAU, 1);
                }

                this.metricManager.LogSocialLoginFailed(1, account, subscriptionId ?? string.Empty, channel, socialPlatform);
                SocialProviderEventSource.Current.ErrorException(requestId, this, nameof(this.CreateOrUpdateUserInfoRecordAsync), OperationStates.Failed, $"Failed to create or update user information for account: {account}, channel: {channel}, platform: {socialPlatform}, channelId: {channelId}", ex);
                if ((ex is ArgumentException) || (ex is SocialChannelExceptionBase) || (ex is QuotaExceededException))
                {
                    throw;
                }

                throw new Exception(string.Format($"Failed to create or update user information for account: {account}, channel: {channel}, platform: {socialPlatform}, channelId: {channelId}"));
            }
        }

        public async Task DeleteUserInfoRecordAsync(string account, string channelName, string channelId, string requestId, CancellationToken cancellationToken)
        {
            var storeAgent = await storeManager.GetStoreAgent();
            var userInfo = await storeAgent.UserInfoStore.GetUserInfoAsync(account, channelName, channelId);
            Validator.IsTrue<ResourceNotFoundException>(userInfo != null, nameof(userInfo), string.Format($"The user information does not exist for account: {account}, channelName: {channelName}, channelId: {channelId}"));

            try
            {
                await storeAgent.UserInfoStore.DeleteUserInfoAsync(account, channelName, channelId);

                // Update history in Social storage
                await this.telemetryManager.CreateSocialLoginHistoryRecordAsync(storeAgent, account, channelName, channelId.ToString(), null, ActionType.Delete.ToString(), DateTime.UtcNow);
                await QuotaCheckClient.ReleaseQuotaAsync(account, Constants.SocialLoginTotal, 1);
                SocialProviderEventSource.Current.Info(requestId, this, nameof(this.DeleteUserInfoRecordAsync), OperationStates.Succeeded, $"account: {account}, channel: {channelName}, channelId: {channelId}");
            }
            catch (Exception ex)
            {
                SocialProviderEventSource.Current.ErrorException(requestId, this, nameof(this.DeleteUserInfoRecordAsync), OperationStates.Failed, $"Failed to delete user information for account: {account}, channel: {channelName}, channelId: {channelId}", ex);
                throw new Exception(string.Format($"Failed to delete user information for account: {account}, channel: {channelName}, channelId: {channelId}"));
            }
        }

        public async Task CreateSocialLoginAccountAsync(string account)
        {
            var subscriptionId = await RequestHelper.GetSubscriptionId(account);
            var storeAgent = await storeManager.GetStoreAgent();
            try
            {
                await this.telemetryManager.CreateSocialLoginAccountAsync(storeAgent, account);
                SocialProviderEventSource.Current.Info(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.CreateSocialLoginAccountAsync), OperationStates.Succeeded, $"account: {account}");
            }
            catch (Exception ex)
            {
                SocialProviderEventSource.Current.ErrorException(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.CreateSocialLoginAccountAsync), OperationStates.Failed, $"Failed to create user information storage table for account {account}", ex);
                throw new Exception(string.Format($"Failed to create user information storage table for account: {account}"));
            }
        }

        public async Task DeleteSocialLoginAccountAsync(string account)
        {
            var storeAgent = await storeManager.GetStoreAgent();
            try
            {
                await storeAgent.UserInfoStore.DeleteSocialLoginAccountDataAsync(account);

                // Delete history in Social storage
                await this.telemetryManager.DeleteSocialLoginAccount(storeAgent, account);
                SocialProviderEventSource.Current.Info(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteSocialLoginAccountAsync), OperationStates.Succeeded, $"account: {account}");
            }
            catch (Exception ex)
            {
                SocialProviderEventSource.Current.ErrorException(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteSocialLoginAccountAsync), OperationStates.Failed, $"Failed to delete user information for account {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete user information for account {account}"));
            }
        }

        private ISocialConnector CreateConnector(string channel)
        {
            ConnectorCreator connectorCreator;
            if (ConnectorCreators.TryGetValue(channel, out connectorCreator))
            {
                return connectorCreator();
            }
            else
            {
                throw new ArgumentException($"Unexpected social login channel '{channel}'");
            }
        }

        // Get credential from tenant service
        private async Task<string> GetCredentialAsync(string account, string channel, string platform)
        {
            var tenant = await TenantCacheClient.GetTenantAsync(account);
            Validator.ArgumentNotNull(tenant, nameof(tenant));
            if (tenant?.TenantDescription?.ChannelSettings != null)
            {
                var setting = tenant.TenantDescription.ChannelSettings.SingleOrDefault(s => s.Type.Equals(channel, StringComparison.InvariantCultureIgnoreCase));

                if (setting != null)
                {
                    string platformKey = platform + "AppId";
                    string appid;
                    setting.Credentials.TryGetValue(platformKey, out appid);
                    Validator.IsTrue<ArgumentException>(appid != null, nameof(appid), "Unexpected social login platform '{0}' for account '{1}' and channel '{2}'", platform, account, channel);
                    return appid;
                }
            }

            throw new ArgumentException($"Unexpected social login channel '{channel}' for account '{account}'");
        }
    }
}