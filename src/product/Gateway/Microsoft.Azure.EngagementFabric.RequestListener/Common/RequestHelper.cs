// <copyright file="RequestHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.TenantCache;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Common
{
    public static class RequestHelper
    {
        public static string ParseAccount(HttpRequestMessage request)
        {
            try
            {
                IEnumerable<string> values;
                request.Headers.TryGetValues(Constants.AccountHeader, out values);
                return values?.FirstOrDefault() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string ParseTrackingId(HttpRequestMessage request)
        {
            try
            {
                IEnumerable<string> values;
                request.Headers.TryGetValues(Constants.OperationTrackingIdHeader, out values);
                return values?.FirstOrDefault() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

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
