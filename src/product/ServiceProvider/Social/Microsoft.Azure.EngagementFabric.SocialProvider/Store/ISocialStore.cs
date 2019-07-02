// <copyright file="ISocialStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Store
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Contract;
    using Microsoft.Azure.EngagementFabric.SocialProvider.EntityFramework;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Telemetry;

    public interface ISocialStore
    {
        Task<UserInfoResult> CreateorUpdateUserInfoAsync(string account, string channelId, UserInfoRecordDescription description);

        Task DeleteUserInfoAsync(string account, string channelName, string channelId);

        Task<UserInfo> GetUserInfoAsync(string account, string channelName, string channelId);

        Task DeleteSocialLoginAccountDataAsync(string account);
    }
}
