// <copyright file="AdminStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.TenantCache;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Store
{
    internal class AdminStore : IAdminStore
    {
        private static readonly ReadOnlyTenantCacheClient TenantCacheClient = ReadOnlyTenantCacheClient.GetClient(true);

        public async Task<KeyValuePair<string, string>[]> GetKeysAsync(string accountName, IEnumerable<string> keyNames)
        {
            var tenant = await TenantCacheClient.GetTenantAsync(accountName);
            if (tenant == null)
            {
                return new KeyValuePair<string, string>[] { };
            }

            if ((tenant.IsDisabled != null && tenant.IsDisabled.Value) ||
                tenant.State != TenantCache.Contract.TenantState.Active)
            {
                throw new AccountDisabledException($"Account {accountName} is disabled");
            }

            return tenant.TenantDescription.AuthenticationRules
                .Where(r => keyNames.Contains(r.KeyName, StringComparer.OrdinalIgnoreCase))
                .SelectMany(r => new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(r.KeyName, r.PrimaryKey),
                    new KeyValuePair<string, string>(r.KeyName, r.SecondaryKey)
                })
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
                .ToArray();
        }
    }
}
