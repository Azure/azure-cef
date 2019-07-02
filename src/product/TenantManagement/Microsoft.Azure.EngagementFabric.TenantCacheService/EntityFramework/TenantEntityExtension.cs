// <copyright file="TenantEntityExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.EntityFramework
{
    internal static class TenantEntityExtension
    {
        public static Tenant ToTenant(this TenantEntity entity)
        {
            var state = TenantState.Unknown;
            Enum.TryParse(entity.State, out state);

            return new Tenant
            {
                SubscriptionId = entity.SubscriptionId,
                ResourceGroupName = entity.ResourceGroup,
                AccountName = entity.AccountName,
                Location = entity.Location,
                SKU = entity.SKU,
                Tags = JsonConvert.DeserializeObject<Dictionary<string, string>>(entity.Tags),
                State = state,
                Address = entity.Address,
                TenantDescription = JsonConvert.DeserializeObject<TenantDescription>(entity.ResourceDescription),
                ResourceId = entity.ResourceId,
                IsDisabled = entity.IsDisabled.HasValue && entity.IsDisabled.Value
            };
        }
    }
}
