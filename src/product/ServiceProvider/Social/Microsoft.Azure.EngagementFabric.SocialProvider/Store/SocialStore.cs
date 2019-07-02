// <copyright file="SocialStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Store
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;
   // using System.Web.Script.Serialization;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Contract;
    using Microsoft.Azure.EngagementFabric.SocialProvider.EntityFramework;
    using Microsoft.WindowsAzure.Storage;

    public class SocialStore : ISocialStore
    {
        private readonly string connectionString;

        public SocialStore(string connectionString, int maxPoolSize)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                MaxPoolSize = maxPoolSize
            };
            connectionStringBuilder.ConnectRetryCount = 3;
            connectionStringBuilder.ConnectRetryInterval = 10;

            var entityStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                Metadata = "res://*/EntityFramework.TargetStoreDataModel.csdl|res://*/EntityFramework.TargetStoreDataModel.ssdl|res://*/EntityFramework.TargetStoreDataModel.msl",
                ProviderConnectionString = connectionStringBuilder.ToString()
            };

            this.connectionString = entityStringBuilder.ConnectionString;
        }

        public async Task<UserInfoResult> CreateorUpdateUserInfoAsync(string account, string channelId, UserInfoRecordDescription description)
        {
            using (var ctx = new UserInfoEntities(this.connectionString))
            {
                UserInfoResult userInfoResult = new UserInfoResult();
                byte[] serializedProfileDescription;
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, description);
                    serializedProfileDescription = stream.ToArray();
                }

                var profile = await ctx.UserInfos.SingleOrDefaultAsync(f => f.Account == account && f.ChannelId == channelId && f.ChannelName == description.Channel);
                var startTime = DateTime.UtcNow;
                if (profile == null)
                {
                    profile = new UserInfo
                    {
                        Account = account,
                        ChannelName = description.Channel,
                        ChannelId = channelId,
                        ChannelProperties = serializedProfileDescription,
                        CreatedTime = startTime,
                        ModifiedTime = startTime
                    };

                    ctx.UserInfos.Add(profile);
                    userInfoResult.Action = ActionType.Create;
                }
                else
                {
                    profile.ChannelProperties = serializedProfileDescription;
                    profile.ModifiedTime = startTime;
                    userInfoResult.Action = ActionType.Update;
                }

                await ctx.SaveChangesAsync();
                userInfoResult.UserInfo = profile;
                return userInfoResult;
            }
        }

        public async Task DeleteUserInfoAsync(string account, string channelName, string channelId)
        {
            using (var ctx = new UserInfoEntities(this.connectionString))
            {
                var entities = await ctx.UserInfos.Where(f => f.Account == account && f.ChannelId == channelId && f.ChannelName == channelName).ToListAsync();
                if (entities == null || !entities.Any())
                {
                    return;
                }

                // Remove profiles
                ctx.UserInfos.RemoveRange(entities);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task DeleteSocialLoginAccountDataAsync(string account)
        {
            using (var ctx = new UserInfoEntities(this.connectionString))
            {
                var entities = await ctx.UserInfos.Where(f => f.Account == account).ToListAsync();
                if (entities == null || !entities.Any())
                {
                    return;
                }

                ctx.UserInfos.RemoveRange(entities);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task<UserInfo> GetUserInfoAsync(string account, string channelName, string channelId)
        {
            using (var ctx = new UserInfoEntities(this.connectionString))
            {
                var entitiy = await ctx.UserInfos.SingleOrDefaultAsync(f => f.Account == account && f.ChannelId == channelId && f.ChannelName == channelName);
                return entitiy;
            }
        }
    }
}
