// <copyright file="ResourceProviderWebService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService
{
    internal sealed class ResourceProviderWebService : StatelessService
    {
        public ResourceProviderWebService(StatelessServiceContext context)
            : base(context)
        {
        }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var protocols = this.ListValidProtocols();

            return this.Context.CodePackageActivationContext.GetEndpoints()
                .Where(endpoint => protocols.Contains(endpoint.Protocol))
                .Select(endpoint => new ServiceInstanceListener(serviceContext => new OwinCommunicationListener(
                        Startup.ConfigureApp,
                        serviceContext,
                        endpoint),
                    endpoint.Name));
        }

        private IEnumerable<EndpointProtocol> ListValidProtocols()
        {
            var protocols = new List<EndpointProtocol>();

            var allowHttp = this.Context
                .CodePackageActivationContext
                .GetConfig<bool>("ResourceProviderWebService", "AllowHttp");

            if (allowHttp)
            {
                protocols.Add(EndpointProtocol.Http);

                ServiceEventSource.Current.ServiceMessage(
                    this.Context,
                    $"HTTP endpoint was enabled");
            }
            else
            {
                ServiceEventSource.Current.ServiceMessage(
                    this.Context,
                    $"HTTP endpoint was disabled");
            }

            var httpsCertThumbprint = this.Context
                .CodePackageActivationContext
                .GetConfig<string>("ResourceProviderWebService", "HttpsCertThumbprint");

            if (!string.IsNullOrWhiteSpace(httpsCertThumbprint))
            {
                using (var store = new X509Store(StoreName.My, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadOnly);

                    var certificates = store.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        httpsCertThumbprint,
                        false);

                    if (certificates.Count > 0)
                    {
                        protocols.Add(EndpointProtocol.Https);

                        ServiceEventSource.Current.ServiceMessage(
                            this.Context,
                            $"HTTPS endpoint was enabled");
                    }
                    else
                    {
                        ServiceEventSource.Current.ServiceMessage(
                            this.Context,
                            $"HTTPS endpoint was disabled: can not find certificate with thumb print {httpsCertThumbprint}");
                    }
                }
            }
            else
            {
                ServiceEventSource.Current.ServiceMessage(
                    this.Context,
                    $"HTTPS endpoint was disabled: no certificate specified");
            }

            return protocols;
        }
    }
}