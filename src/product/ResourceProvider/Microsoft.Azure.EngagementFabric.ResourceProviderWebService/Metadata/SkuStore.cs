// <copyright file="SkuStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata
{
    internal static class SkuStore
    {
        public const string BasicTierName = "Basic";
        public const string B1SkuName = "B1";
        public const string StandardTierName = "Standard";
        public const string S1SkuName = "S1";
        public const string PremiumTierName = "Premium";
        public const string P1SkuName = "P1";

        public static readonly SKU B1 = new SKU
        {
            Name = B1SkuName,
            Tier = BasicTierName
        };

        public static readonly SKU S1 = new SKU
        {
            Name = S1SkuName,
            Tier = StandardTierName
        };

        public static readonly SKU P1 = new SKU
        {
            Name = P1SkuName,
            Tier = PremiumTierName
        };

        public static readonly IEnumerable<object> DefaultRestrictions = new object[] { };

        public static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> Quotas = new Dictionary<string, IReadOnlyDictionary<string, int>>(StringComparer.OrdinalIgnoreCase)
        {
            {
                B1.Name,
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        Constants.SocialLoginMAU,
                        100000
                    },
                    {
                        Constants.SocialLoginTotal,
                        10000000
                    }
                }
            },
            {
                S1.Name,
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        Constants.SocialLoginMAU,
                        1000000
                    },
                    {
                        Constants.SocialLoginTotal,
                        10000000
                    }
                }
            },
            {
                P1.Name,
                new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    {
                        Constants.SocialLoginMAU,
                        10000000
                    },
                    {
                        Constants.SocialLoginTotal,
                        10000000
                    }
                }
            }
        };

        private static readonly IEnumerable<SKU> AllSKUs = new[]
        {
            B1,
            S1,
            P1
        };

        public static IEnumerable<SkuDescription> Descriptions { get; private set; }

        public static void Initialize(
            IEnumerable<string> locations,
            IEnumerable<string> skus)
        {
            Descriptions = AllSKUs
                .Where(u => skus.Contains(u.Name))
                .Select(u => new SkuDescription
                {
                    ResourceType = NameStore.FullyQualifiedAccountResourceType,
                    Name = u.Name,
                    Tier = u.Tier,
                    Locations = locations,
                    LocationInfo = locations.Select(location => new SkuLocationInfoItem
                    {
                        Location = location
                    }),
                    Restrictions = DefaultRestrictions
                })
                .ToList();
        }

        public static SKU GetSKU(string name)
        {
            var description = Descriptions.Single(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));

            return new SKU
            {
                Name = description.Name,
                Tier = description.Tier
            };
        }
    }
}