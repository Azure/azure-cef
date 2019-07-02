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
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using IServiceProvider = Microsoft.Azure.EngagementFabric.ProviderInterface.IServiceProvider;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Manager
{
    public static class ProviderManager
    {
        public static IServiceProvider GetSmsServiceProvider()
        {
            var serviceType = "fabric:/SmsApp/SmsProvider";
            var proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(
                    serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });
            var client = proxyFactory.CreateServiceProxy<IServiceProvider>(new Uri(serviceType));
            return client;
        }
    }
}
