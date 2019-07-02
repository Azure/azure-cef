// -----------------------------------------------------------------------
// <copyright file="OtpProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.OtpProvider
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.Common.Extension;
    using Microsoft.Azure.EngagementFabric.Common.Serialization;
    using Microsoft.Azure.EngagementFabric.Common.ServiceProvider;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Configuration;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Engine;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Monitor;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Scheduler;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Store;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Telemetry;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using IServiceProvider = Microsoft.Azure.EngagementFabric.ProviderInterface.IServiceProvider;

    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public sealed partial class OtpProvider : StatelessService, IServiceProvider
    {
        private IOtpEngine engine;
        private StoreTaskScheduler taskScheduler;
        private RestfulDispatcher<ServiceProviderResponse> dispatcher;
        private ServiceConfiguration configuration;
        private MetricManager metricManager;

        public OtpProvider(StatelessServiceContext context)
            : base(context)
        {
            this.configuration = new ServiceConfiguration(FabricRuntime.GetNodeContext(), context.CodePackageActivationContext);
            this.metricManager = new MetricManager(configuration);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new[]
            {
                new ServiceInstanceListener((c) =>
                {
                    return new FabricTransportServiceRemotingListener(c, this, null, new ServiceRemotingJsonSerializationProvider());
                })
            };
        }

        /// <summary>
        /// This is the main entry point for your service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        /// <returns>Completed task</returns>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var storeConnectionString = this.configuration.DefaultConnectionString;
            var storeMaxPoolSize = this.configuration.DefaultConnectionPoolSize;
            var storageConnectionString = this.configuration.TelemetryStoreConnectionString;
            this.taskScheduler = new StoreTaskScheduler(
                new OtpStoreFactory(storeConnectionString, Convert.ToInt32(storeMaxPoolSize)),
                storageConnectionString);
            this.taskScheduler.TimerStart();

            this.engine = new OtpEngine(
                message => ServiceEventSource.Current.ServiceMessage(this.Context, message),
                new OtpStoreFactory(storeConnectionString, Convert.ToInt32(storeMaxPoolSize)),
                new TelemetryManager(storageConnectionString),
                this.metricManager);

            this.dispatcher = new RestfulDispatcher<ServiceProviderResponse>(
                message => ServiceEventSource.Current.ServiceMessage(this.Context, message),
                this);

            await Task.CompletedTask;
        }
    }
}