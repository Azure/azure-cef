// <copyright file="OperationStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata
{
    internal static class OperationStore
    {
        public static readonly IEnumerable<Operation> Operations = new[]
        {
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedAccountResourceType}/read",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.AccountResourceType,
                    Operation = "List or get the EngagementFabric account",
                    Description = "List or get the EngagementFabric account"
                }
            },
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedAccountResourceType}/write",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.AccountResourceType,
                    Operation = "Create or update the EngagementFabric account",
                    Description = "Create or update the EngagementFabric account"
                }
            },
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedAccountResourceType}/delete",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.AccountResourceType,
                    Operation = "Delete the EngagementFabric account",
                    Description = "Delete the EngagementFabric account"
                }
            },
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedAccountResourceType}/ListKeys/action",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.AccountResourceType,
                    Operation = "Get all keys of the EngagementFabric account",
                    Description = "Get all keys of the EngagementFabric account"
                }
            },
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedAccountResourceType}/RegenerateKey/action",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.AccountResourceType,
                    Operation = "Regenerate the EngagementFabric account key",
                    Description = "Regenerate the EngagementFabric account key"
                }
            },
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedAccountResourceType}/ListChannelTypes/action",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.AccountResourceType,
                    Operation = "List available EngagementFabric channel types and functions",
                    Description = "List available EngagementFabric channel types and functions"
                }
            },
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedChannelResourceType}/read",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.ChannelResourceType,
                    Operation = "List or get the EngagementFabric channel",
                    Description = "List or get the EngagementFabric channel"
                }
            },
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedChannelResourceType}/write",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.ChannelResourceType,
                    Operation = "Create or update the EngagementFabric channel",
                    Description = "Create or update the EngagementFabric channel"
                }
            },
            new Operation
            {
                Name = $"{NameStore.FullyQualifiedChannelResourceType}/delete",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.ChannelResourceType,
                    Operation = "Delete the EngagementFabric channel",
                    Description = "Delete the EngagementFabric channel"
                }
            },
            new Operation
            {
                Name = $"{NameStore.ProviderNamespace}/checkNameAvailability/action",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.AccountResourceType,
                    Operation = "Check name availability",
                    Description = "Check name availability"
                }
            },
            new Operation
            {
                Name = $"{NameStore.ProviderNamespace}/operations/read",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = "Operations",
                    Operation = "List available operations",
                    Description = "List available operations"
                }
            },
            new Operation
            {
                Name = $"{NameStore.ProviderNamespace}/skus/read",
                Display = new OperationDisplay
                {
                    Provder = NameStore.ServiceDescription,
                    Resource = NameStore.AccountResourceType,
                    Operation = "List available SKUs",
                    Description = "List available SKUs"
                }
            }
        };
    }
}