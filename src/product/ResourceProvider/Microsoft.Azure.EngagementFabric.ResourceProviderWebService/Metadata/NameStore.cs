// <copyright file="NameStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata
{
    internal static class NameStore
    {
        public const string ServiceTitle = "EngagementFabric";

        public const string ServiceDescription = "Microsoft Customer Engagement Fabric";

        public const string ProviderNamespace = "Microsoft.EngagementFabric";

        public const string AccountResourceType = "Accounts";

        public const string FullyQualifiedAccountResourceType = ProviderNamespace + "/" + AccountResourceType;

        public const string ChannelResourceType = "Channels";

        public const string FullyQualifiedChannelResourceType = ProviderNamespace + "/" + AccountResourceType + "/" + ChannelResourceType;

        public const string Subscriptions = "subscriptions";

        public const string ResourceGroups = "resourceGroups";

        public const string Providers = "providers";
    }
}