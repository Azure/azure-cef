// -----------------------------------------------------------------------
// <copyright file="ProviderManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using IServiceProvider = Microsoft.Azure.EngagementFabric.ProviderInterface.IServiceProvider;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Manager
{
    // TODO: refine for provider onboarding and managing
    public static class ProviderManager
    {
        public const string SocialProviderType = "SocialLogin";
        public const string SmsProviderType = "Sms";
        public const string OtpProviderType = "Otp";
        public const string EmailProviderType = "Email";

        private static readonly Dictionary<string, string> AllProviderServiceTypes = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
        {
            {
                SocialProviderType,
                "fabric:/SocialApp/SocialProvider"
            },
            {
                SmsProviderType,
                "fabric:/SmsApp/SmsProvider"
            },
            {
                OtpProviderType,
                "fabric:/OtpApp/OtpProvider"
            },
            {
                EmailProviderType,
                "fabric:/EmailApp/EmailProvider"
            }
        };

        private static ConcurrentDictionary<string, IServiceProvider> providers = new ConcurrentDictionary<string, IServiceProvider>(StringComparer.OrdinalIgnoreCase);

        public static IList<IServiceProvider> GetAllProviders()
        {
            return AllProviderServiceTypes.Keys.Select(t => GetServiceProvider(t)).Where(p => p != null).ToList();
        }

        public static IServiceProvider GetServiceProvider(string providerType)
        {
            if (providers.ContainsKey(providerType))
            {
                return providers[providerType];
            }

            var serviceType = GetProviderServiceType(providerType);
            var proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(
                    serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });
            var client = proxyFactory.CreateServiceProxy<IServiceProvider>(new Uri(serviceType));
            if (!providers.TryAdd(providerType, client))
            {
                providers.TryGetValue(providerType, out client);
            }

            return client;
        }

        private static string GetProviderServiceType(string providerType)
        {
            string serviceType;
            if (AllProviderServiceTypes.TryGetValue(providerType, out serviceType))
            {
                return serviceType;
            }

            throw new ArgumentException($"The provider type '{providerType}' is invalid.");
        }
    }
}
