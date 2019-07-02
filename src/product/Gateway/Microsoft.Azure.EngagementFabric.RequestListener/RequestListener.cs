// -----------------------------------------------------------------------
// <copyright file="RequestListener.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Fabric;
using System.Fabric.Description;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Security;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.RequestListener.Manager;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Azure.EngagementFabric.RequestListener
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName", Justification = "Keep using name of RequestListnerService")]
    public sealed class RequestListenerService : StatelessService
    {
        private TenantManager tenantManager;

        public RequestListenerService(StatelessServiceContext context)
            : base(context)
        {
            RequestListenerService.ServiceConfiguration = new ServiceConfiguration(FabricRuntime.GetNodeContext(), Context.CodePackageActivationContext);
            RequestListenerService.AuditClient = new AuditClient("Gateway");
        }

        public static ServiceConfiguration ServiceConfiguration { get; private set; }

        public static AuditClient AuditClient { get; private set; }

        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            var onlyHttps = RequestListenerService.ServiceConfiguration.OnlyHttps;
            var endpoints = Context.CodePackageActivationContext.GetEndpoints()
                                   .Where(endpoint => endpoint.Protocol == EndpointProtocol.Https || (endpoint.Protocol == EndpointProtocol.Http && !onlyHttps))
                                   .Select(endpoint => endpoint.Name);

            return endpoints.Select(endpoint => new ServiceInstanceListener(
                serviceContext => new OwinCommunicationListener(
                    (b) => Startup.ConfigureApp(b),
                    serviceContext,
                    ServiceEventSource.Current,
                    endpoint), endpoint));
        }

        protected override Task RunAsync(CancellationToken cancellationToken)
        {
            this.tenantManager = new TenantManager();
            return Task.CompletedTask;
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            this.tenantManager.Dispose();
            return Task.CompletedTask;
        }
    }
}
