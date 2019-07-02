// <copyright file="ChannelTypeStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata
{
    internal static class ChannelTypeStore
    {
        public static readonly IEnumerable<ChannelTypeDescription> Descriptions = new[]
        {
            new ChannelTypeDescription
            {
                ChannelType = "QQ",
                ChannelDescription = "QQ",
                ChannelFunctions = new[]
                {
                    "Social"
                }
            },
            new ChannelTypeDescription
            {
                ChannelType = "WeChat",
                ChannelDescription = "WeChat",
                ChannelFunctions = new[]
                {
                    "Social"
                }
            },
            new ChannelTypeDescription
            {
                ChannelType = "Weibo",
                ChannelDescription = "Weibo",
                ChannelFunctions = new[]
                {
                    "Social"
                }
            }
        };

        public static readonly IReadOnlyDictionary<string, IEnumerable<string>> MaskFreeCredentialKeys = new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "QQ",
                new[]
                {
                    "iosAppId",
                    "androidAppId"
                }
            }
        };
    }
}