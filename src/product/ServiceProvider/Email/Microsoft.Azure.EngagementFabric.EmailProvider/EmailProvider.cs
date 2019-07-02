// <copyright file="EmailProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.Common.ServiceProvider;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.EmailProvider.Billing;
using Microsoft.Azure.EngagementFabric.EmailProvider.Configuration;
using Microsoft.Azure.EngagementFabric.EmailProvider.Controller;
using Microsoft.Azure.EngagementFabric.EmailProvider.Credential;
using Microsoft.Azure.EngagementFabric.EmailProvider.Engine;
using Microsoft.Azure.EngagementFabric.EmailProvider.Report;
using Microsoft.Azure.EngagementFabric.EmailProvider.Scheduler;
using Microsoft.Azure.EngagementFabric.EmailProvider.Store;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using IServiceProvider = Microsoft.Azure.EngagementFabric.ProviderInterface.IServiceProvider;

namespace Microsoft.Azure.EngagementFabric.EmailProvider
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public sealed partial class EmailProvider : StatelessService, IServiceProvider, IReportingService
    {
        private ServiceConfiguration configuration;
        private IEmailStoreFactory storeFactory;
        private IEmailEngine engine;
        private RestfulDispatcher<ServiceProviderResponse> dispatcher;

        private IReportManager reportManager;

        private ICredentialManager credentialManager;
        private BillingAgent billingAgent;
        private MetricManager metricManager;
        private OperationController controller;

        private StorageTaskScheduler storageTaskScheduler;

        public EmailProvider(StatelessServiceContext context)
            : base(context)
        {
            var nodeContext = FabricRuntime.GetNodeContext();
            this.configuration = new ServiceConfiguration(nodeContext, context.CodePackageActivationContext);
            this.storeFactory = new EmailStoreFactory(configuration.DefaultConnectionString);
            this.billingAgent = new BillingAgent();
            this.metricManager = new MetricManager(configuration);
            this.credentialManager = new CredentialManager(this.storeFactory);

            this.reportManager = new ReportManager(
                nodeContext.NodeName,
                this.storeFactory,
                this.configuration,
                this.billingAgent,
                this.metricManager,
                this.credentialManager);

           this.engine = new EmailEngine(
               this.storeFactory,
               this.configuration,
               this.metricManager,
               this.credentialManager);
        }

        /// <summary>
        /// This is the callback once DispatcherService complete dispatching
        /// </summary>
        /// <param name="results">Dispatcher results</param>
        /// <returns>N/A</returns>
        public async Task ReportDispatcherResultsAsync(ReadOnlyCollection<OutputResult> results)
        {
            try
            {
                if (results == null || results.Count <= 0)
                {
                    return;
                }

                var tasks = results.Select(r => this.reportManager.OnDispatchCompleteAsync(r));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.ReportDispatcherResultsAsync), OperationStates.Failed, "Failed to dispatch report", ex);
            }
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
        /// <returns>Async task</returns>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            this.controller = new OperationController(
                    this.storeFactory,
                    this.engine,
                    this.configuration,
                    this.reportManager,
                    this.credentialManager,
                    this.metricManager);

            this.storageTaskScheduler = new StorageTaskScheduler(this.reportManager);
            this.storageTaskScheduler.TimerStart();

            this.dispatcher = new RestfulDispatcher<ServiceProviderResponse>(
                message => ServiceEventSource.Current.ServiceMessage(this.Context, message), this.controller);

            await this.billingAgent.OnRunAsync(cancellationToken);
        }
    }
}
