// <copyright file="ISocialEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Engine
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Contract;

    public interface ISocialEngine
    {
        Task<UserInfoRecord> CreateOrUpdateUserInfoRecordAsync(string account, string channel, string accessToken, string channelId, string socialPlatform, string requestId, CancellationToken cancellationToken);

        Task DeleteUserInfoRecordAsync(string account, string channelName, string channelId, string requestId, CancellationToken cancellationToken);

        Task CreateSocialLoginAccountAsync(string account);

        Task DeleteSocialLoginAccountAsync(string account);
    }
}
