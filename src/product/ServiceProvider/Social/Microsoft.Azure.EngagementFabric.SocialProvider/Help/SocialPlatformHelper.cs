// <copyright file="SocialPlatformHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Helper
{
    public static class SocialPlatformHelper
    {
        public const string Android = "android";
        public const string IOS = "ios";

        public static string Format(string platform)
        {
            if (Android.Equals(platform, StringComparison.OrdinalIgnoreCase))
            {
                return Android;
            }

            if (IOS.Equals(platform, StringComparison.OrdinalIgnoreCase))
            {
                return IOS;
            }

            throw new ArgumentException($"platform '{platform}' is not supported.");
        }

        public static List<string> GetAppPlatforms()
        {
            return new List<string>
            {
                SocialPlatformHelper.Android,
                SocialPlatformHelper.IOS
            };
        }
    }
}
