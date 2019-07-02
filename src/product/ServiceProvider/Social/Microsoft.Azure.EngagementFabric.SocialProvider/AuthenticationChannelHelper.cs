// <copyright file="AuthenticationChannelHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.AuthenticationProvider.Helper
{
    public static class AuthenticationChannelHelper
    {
        public const string WeChat = "WeChat";
        public const string QQ = "QQ";
        public const string Weibo = "Weibo";

        public static string Format(string channel)
        {
            if (WeChat.Equals(channel, StringComparison.OrdinalIgnoreCase))
            {
                return WeChat;
            }

            if (QQ.Equals(channel, StringComparison.OrdinalIgnoreCase))
            {
                return QQ;
            }

            if (Weibo.Equals(channel, StringComparison.OrdinalIgnoreCase))
            {
                return Weibo;
            }

            throw new ArgumentException($"Channel {channel} is not supported.");
        }

        public static List<string> GetAppChannels()
        {
            return new List<string>
            {
                AuthenticationChannelHelper.WeChat,
                AuthenticationChannelHelper.QQ,
                AuthenticationChannelHelper.Weibo
            };
        }
    }
}
