// <copyright file="SocialLoginHistoryTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Telemetry
{
    public class SocialLoginHistoryTableEntity : TableEntity
    {
        public SocialLoginHistoryTableEntity()
            : base()
        {
        }

        public SocialLoginHistoryTableEntity(string account, string channel, string channelId, string platform, string action, DateTime timeStart)
        {
            this.PartitionKey = timeStart.ToUniversalTime().Ticks.ToString();
            this.RowKey = channelId;
            Account = account;
            ChannelName = channel;
            ChannelId = channelId;
            Platform = platform;
            ActionTime = timeStart.ToUniversalTime();
            Action = action;
        }

        public string ChannelId { get; set; }

        public string Account { get; set; }

        public string ChannelName { get; set; }

        public string Platform { get; set; }

        public string Action { get; set; }

        public DateTime ActionTime { get; set; }

        internal static async Task InsertSocialLoginHistoryTableEntity(CloudTable table, SocialLoginHistoryTableEntity summary)
        {
            var operation = TableOperation.Insert(summary);
            var tableInsert = table.ExecuteAsync(operation);
            await Task.CompletedTask;
        }
    }
}
