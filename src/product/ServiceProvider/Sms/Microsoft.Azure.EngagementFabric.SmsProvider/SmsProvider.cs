// <copyright file="SmsProvider.cs" company="Microsoft Corporation">
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
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.SmsProvider.Billing;
using Microsoft.Azure.EngagementFabric.SmsProvider.Configuration;
using Microsoft.Azure.EngagementFabric.SmsProvider.Controller;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Microsoft.Azure.EngagementFabric.SmsProvider.Inbound;
using Microsoft.Azure.EngagementFabric.SmsProvider.Mdm;
using Microsoft.Azure.EngagementFabric.SmsProvider.Report;
using Microsoft.Azure.EngagementFabric.SmsProvider.Store;
//using Microsoft.Cloud.Metrics.Client;
//using Microsoft.Cloud.Metrics.Client.Metrics;
//using Microsoft.Online.Metrics.Serialization.Configuration;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using IServiceProvider = Microsoft.Azure.EngagementFabric.ProviderInterface.IServiceProvider;

namespace Microsoft.Azure.EngagementFabric.SmsProvider
{
    /// <summary>
    /// An instance of this class is created for each service instance by the Service Fabric runtime.
    /// </summary>
    public sealed partial class SmsProvider : StatelessService, IServiceProvider, IReportingService
    {
        private readonly ServiceConfiguration configuration;
        private readonly ISmsStoreFactory storeFactory;
        private readonly BillingAgent billingAgent;
        private readonly MetricManager metricManager;
        // private readonly ITimeSeriesManager timeSeriesManager;
        private readonly ICredentialManager credentialManager;
        private readonly IReportManager reportManager;
        private readonly IInboundManager inboundManager;

        private RestfulDispatcher<ServiceProviderResponse> dispatcher;
        private OperationController controller;

        public SmsProvider(StatelessServiceContext context)
            : base(context)
        {
            this.configuration = new ServiceConfiguration(FabricRuntime.GetNodeContext(), context.CodePackageActivationContext);
            this.storeFactory = new SmsStoreFactory(this.configuration.DefaultConnectionString);
            this.billingAgent = new BillingAgent();
            this.metricManager = new MetricManager(this.configuration);

            // this.timeSeriesManager = this.BuildTimeSeriesManager();

            this.credentialManager = new CredentialManager(this.storeFactory);
            this.reportManager = new ReportManager(
                this.storeFactory,
                this.configuration,
                this.billingAgent,
                this.metricManager,
                this.credentialManager);
            this.inboundManager = new InboundManager(
                this.storeFactory,
                this.configuration,
                this.reportManager,
                this.credentialManager);
        }

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
                SmsProviderEventSource.Current.ErrorException(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.ReportDispatcherResultsAsync), OperationStates.Failed, "Failed to dispatch report", ex);
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
                // Listener for service remoting
                new ServiceInstanceListener(
                    (c) =>
                    {
                        return new FabricTransportServiceRemotingListener(c, this, null, new ServiceRemotingJsonSerializationProvider());
                    },
                    "InternalListener"),

                // Listener for https callback
                new ServiceInstanceListener(
                    serviceContext => new OwinCommunicationListener(
                        (b) => Startup.ConfigureApp(b, this.inboundManager),
                        serviceContext,
                        ServiceEventSource.Current,
                        "ServiceEndpointHttps"),
                    "ExternalListener")
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
                    this.configuration,
                    this.reportManager,
                    this.inboundManager,
                    this.credentialManager,
                    this.metricManager);

            this.dispatcher = new RestfulDispatcher<ServiceProviderResponse>(
                message => ServiceEventSource.Current.ServiceMessage(this.Context, message), this.controller);

            try
            {
                await Task.WhenAll(
                    this.billingAgent.OnRunAsync(cancellationToken)
                    /*,this.ArchiveMetricsAsync(cancellationToken) removed by jin*/);
            }
            catch (OperationCanceledException)
            {
            }
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            this.reportManager.Dispose();
            this.inboundManager.Dispose();

            await this.billingAgent.OnCloseAsync();
            await base.OnCloseAsync(cancellationToken);
        }

        private async Task ArchiveMetricsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    //  (todo jin)post-fix 
                    // await this.timeSeriesManager.TryArchiveTimeSeriesAsync(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                }
                catch (Exception ex)
                {
                    SmsProviderEventSource.Current.ErrorException(
                        EventSourceBase.EmptyTrackingId,
                        this,
                        nameof(this.ArchiveMetricsAsync),
                        OperationStates.Failed,
                        $"Failed to archive metrics",
                        ex);
                }

                await Task.Delay(this.configuration.MdmArchiveInterval, cancellationToken);
            }
        }

        private ITimeSeriesManager BuildTimeSeriesManager()
        {
            //  comment by jin
            return null;

            //IEnumerable<TimeSeriesReader> readers;
            //if (this.configuration.MdmCertificate == null)
            //{
            //    SmsProviderEventSource.Current.Warning(
            //        EventSourceBase.EmptyTrackingId,
            //        this,
            //        nameof(this.BuildTimeSeriesManager),
            //        OperationStates.Empty,
            //        $"Connect to MDM dropped due to absence of the certificate");

            //    readers = new TimeSeriesReader[] { };
            //}
            //else
            //{
            //    var metricReader = new MetricReader(new ConnectionInfo(
            //        this.configuration.MdmCertificate,
            //        MdmEnvironment.Production));

            //    readers = MetricManager.MetricNames
            //        .Select(metricName => new MetricIdentifier(
            //            this.configuration.MdmAccount,
            //            this.configuration.MdmMetricNamespace,
            //            metricName))
            //        .Select(metricId => new TimeSeriesReader(
            //            metricReader,
            //            metricId))
            //        .ToList();
            //}

            //var store = new ArchivedTimeSeriesStore(
            //    this.configuration.TelemetryStoreConnectionString,
            //    "archivedMetrics",
            //    string.Empty);

            //var dimensionNames = new[]
            //{
            //    MetricManager.DimensionEngagementAccount,
            //    MetricManager.DimensionMessageCategory
            //};

            //return new TimeSeriesManager(
            //    readers,
            //    dimensionNames,
            //    store);
        }
    }
}
