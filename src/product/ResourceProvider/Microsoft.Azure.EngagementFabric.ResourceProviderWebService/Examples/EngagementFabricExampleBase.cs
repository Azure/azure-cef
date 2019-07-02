// <copyright file="EngagementFabricExampleBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.ExampleHelper;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples
{
    internal class EngagementFabricExampleBase : ExampleBase
    {
        protected const string DefaultSubscriptionId = "EDBF0095-A524-4A84-95FB-F72DA41AA6A1";
        protected const string DefaultResourceGroupName = "ExampleRg";
        protected const string DefaultAccountName = "ExampleAccount";
        protected const string DefaultChannelName = "ExampleChannel";
        protected const string DefaultLocation = "WestUS";

        protected static readonly ChannelProperties DefaultChannelProperties = new ChannelProperties
        {
            ChannelType = "MockChannel",
            ChannelFunctions = new[]
            {
                "MockFunction1",
                "MockFunction2"
            },
            Credentials = new Dictionary<string, string>
            {
                { "AppId", "exampleApp" },
                { "AppKey", "exampleAppKey" }
            }
        };

        protected static readonly Channel DefaultResponseChannel = new Channel
        {
            Id = $"subscriptions/{DefaultSubscriptionId}/resourceGroups/{DefaultResourceGroupName}/providers/{NameStore.FullyQualifiedAccountResourceType}/{DefaultAccountName}/{NameStore.ChannelResourceType}/{DefaultChannelName}",
            Name = DefaultChannelName,
            Type = NameStore.FullyQualifiedChannelResourceType,
            Properties = new ChannelProperties
            {
                ChannelType = DefaultChannelProperties.ChannelType,
                ChannelFunctions = DefaultChannelProperties.ChannelFunctions,
                Credentials = DefaultChannelProperties.Credentials.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Key == "AppKey" ? string.Empty : pair.Value)
            }
        };

        protected static readonly IEnumerable<Channel> DefaultResponseChannels = new[]
        {
            DefaultResponseChannel,
            new Channel
            {
                Id = $"subscriptions/{DefaultSubscriptionId}/resourceGroups/{DefaultResourceGroupName}/providers/{NameStore.FullyQualifiedAccountResourceType}/{DefaultAccountName}/{NameStore.ChannelResourceType}/ExampleChannel2",
                Name = "ExampleChannel2",
                Type = NameStore.FullyQualifiedChannelResourceType,
                Properties = new ChannelProperties
                {
                    ChannelType = "MockChannel2",
                    ChannelFunctions = new[]
                    {
                        "MockFunction1",
                        "MockFunction3"
                    },
                    Credentials = new Dictionary<string, string>
                    {
                        { "AppId", "exampleApp2" },
                        { "AppKey", string.Empty }
                    }
                }
            }
        };

        protected static readonly Account DefaultAccount = new Account
        {
            Id = $"subscriptions/{DefaultSubscriptionId}/resourceGroups/{DefaultResourceGroupName}/providers/{NameStore.FullyQualifiedAccountResourceType}/{DefaultAccountName}",
            Name = DefaultAccountName,
            Type = NameStore.FullyQualifiedAccountResourceType,
            Location = DefaultLocation,
            SKU = SkuStore.B1
        };

        protected static readonly IEnumerable<Account> DefaultAccounts = new[]
        {
            DefaultAccount,
            new Account
            {
                Id = $"subscriptions/{DefaultSubscriptionId}/resourceGroups/{DefaultResourceGroupName}/providers/{NameStore.FullyQualifiedAccountResourceType}/ExampleAccount2",
                Name = "ExampleAccount2",
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = DefaultLocation,
                SKU = SkuStore.S1,
            },
            new Account
            {
                Id = $"subscriptions/{DefaultSubscriptionId}/resourceGroups/{DefaultResourceGroupName}/providers/{NameStore.FullyQualifiedAccountResourceType}/ExampleAccount3",
                Name = "ExampleAccount3",
                Type = NameStore.FullyQualifiedAccountResourceType,
                Location = DefaultLocation,
                SKU = SkuStore.P1,
            }
        };

        protected static readonly IEnumerable<KeyDescription> DefaultKeys = new[]
        {
            new KeyDescription
            {
                Name = "Full",
                Rank = KeyRank.PrimaryKey,
                Value = "<ExampleFullPrimaryKeyValue>"
            },
            new KeyDescription
            {
                Name = "Full",
                Rank = KeyRank.SecondaryKey,
                Value = "<ExampleFullSecondaryKeyValue>"
            },
            new KeyDescription
            {
                Name = "Device",
                Rank = KeyRank.PrimaryKey,
                Value = "<ExampleDevicePrimaryKeyValue>"
            },
            new KeyDescription
            {
                Name = "Device",
                Rank = KeyRank.SecondaryKey,
                Value = "<ExampleDeviceSecondaryKeyValue>"
            }
        };

        protected static readonly IEnumerable<ChannelTypeDescription> DefaultChannelTypeDescriptions = new[]
        {
            new ChannelTypeDescription
            {
                ChannelType = "MockChannel1",
                ChannelDescription = "Description of mockChannel1",
                ChannelFunctions = new[]
                {
                    "MockFunction1",
                    "MockFunction2",
                }
            },
            new ChannelTypeDescription
            {
                ChannelType = "MockChannel2",
                ChannelDescription = "Description of mockChannel2",
                ChannelFunctions = new[]
                {
                    "MockFunction1",
                    "MockFunction3",
                }
            },
            new ChannelTypeDescription
            {
                ChannelType = "MockChannel3",
                ChannelDescription = "Description of mockChannel3",
                ChannelFunctions = new[]
                {
                    "MockFunction1",
                    "MockFunction2",
                    "MockFunction3",
                }
            },
        };

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; protected set; }

        [JsonProperty("resourceGroupName")]
        public string ResourceGroupName { get; protected set; }

        [JsonProperty("accountName")]
        public string AccountName { get; protected set; }

        [JsonProperty("channelName")]
        public string ChannelName { get; protected set; }

        [JsonProperty("api-version")]
        public string ApiVersion { get; protected set; }
    }
}
