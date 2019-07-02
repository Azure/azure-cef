// <copyright file="SocialChannelHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Helper
{
    public static class SocialChannelHelper
    {
        public const string WeChat = "wechat";
        public const string QQ = "qq";
        public const string Weibo = "weibo";

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

            throw new ArgumentException($"Channel '{channel}' is not supported.");
        }
    }
}
