// <copyright file="RequestHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.TenantCache;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Common
{
    public static class RequestHelper
    {
        public static async Task<string> GetSubscriptionId(string account)
        {
            try
            {
                if (string.IsNullOrEmpty(account))
                {
                    return string.Empty;
                }

                var client = ReadOnlyTenantCacheClient.GetClient(true);
                var tenant = await client.GetTenantAsync(account);
                return tenant?.SubscriptionId ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
