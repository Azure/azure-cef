// <copyright file="QuotaCheckClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Azure.EngagementFabric.TenantCache
{
    public class QuotaCheckClient
    {
        private const string ServiceUri = "fabric:/TenantManagementApp/TenantCacheService";

        private static readonly object InitializeLock = new object();

        private static ITenantCache service;

        public static async Task CreateOrUpdateQuotaAsync(
            string engagementAccount,
            string quotaName,
            int count)
        {
            Validator.ArgumentNotNullOrEmpty(engagementAccount, nameof(engagementAccount));
            Validator.ArgumentNotNullOrEmpty(quotaName, nameof(quotaName));

            EnsureService();

            await service.CreateOrUpdateQuotaAsync(
                engagementAccount,
                quotaName,
                count);
        }

        public static async Task RemoveQuotaAsync(
            string engagementAccount,
            string quotaName)
        {
            Validator.ArgumentNotNullOrEmpty(engagementAccount, nameof(engagementAccount));
            Validator.ArgumentNotNullOrEmpty(quotaName, nameof(quotaName));

            EnsureService();

            await service.RemoveQuotaAsync(
                engagementAccount,
                quotaName);
        }

        public static async Task<QuotaOperationResult> AcquireQuotaAsync(
            string engagementAccount,
            string quotaName,
            int required)
        {
            Validator.ArgumentNotNullOrEmpty(engagementAccount, nameof(engagementAccount));
            Validator.ArgumentNotNullOrEmpty(quotaName, nameof(quotaName));

            EnsureService();

            var result = await service.AcquireQuotaAsync(
                engagementAccount,
                quotaName,
                required);

            if (result.Status != HttpStatusCode.OK)
            {
                throw new QuotaExceededException(
                    engagementAccount,
                    quotaName,
                    result.Remaining);
            }

            return result;
        }

        public static async Task<QuotaOperationResult> ReleaseQuotaAsync(
            string engagementAccount,
            string quotaName,
            int released)
        {
            Validator.ArgumentNotNullOrEmpty(engagementAccount, nameof(engagementAccount));
            Validator.ArgumentNotNullOrEmpty(quotaName, nameof(quotaName));

            EnsureService();

            return await service.ReleaseQuotaAsync(
                engagementAccount,
                quotaName,
                released);
        }

        private static void EnsureService()
        {
            lock (InitializeLock)
            {
                if (service == null)
                {
                    var proxyFactory = new ServiceProxyFactory((c) =>
                    {
                        return new FabricTransportServiceRemotingClientFactory(serializationProvider: new ServiceRemotingJsonSerializationProvider());
                    });

                    service = proxyFactory.CreateServiceProxy<ITenantCache>(new Uri(ServiceUri));
                }
            }
        }
    }
}