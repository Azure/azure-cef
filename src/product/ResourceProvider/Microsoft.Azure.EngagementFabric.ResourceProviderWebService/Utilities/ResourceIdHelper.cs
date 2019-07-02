// <copyright file="ResourceIdHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities
{
    internal static class ResourceIdHelper
    {
        public static string GetAccountId(
            string subscriptionId,
            string resourceGroupName,
            string accountName)
        {
            return $"/{NameStore.Subscriptions}/{subscriptionId}/{NameStore.ResourceGroups}/{resourceGroupName}/{NameStore.Providers}/{NameStore.FullyQualifiedAccountResourceType}/{accountName}";
        }

        public static string GetChannelId(
            string subscriptionId,
            string resourceGroupName,
            string accountName,
            string channelName)
        {
            return $"/{NameStore.Subscriptions}/{subscriptionId}/{NameStore.ResourceGroups}/{resourceGroupName}/{NameStore.Providers}/{NameStore.FullyQualifiedAccountResourceType}/{accountName}/{NameStore.ChannelResourceType}/{channelName}";
        }
    }
}
