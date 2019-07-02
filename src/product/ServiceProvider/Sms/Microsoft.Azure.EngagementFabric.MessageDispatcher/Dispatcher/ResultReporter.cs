// <copyright file="ResultReporter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.DispatcherInterface;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public class ResultReporter : BaseComponent, IResultReporter
    {
        private ServiceProxyFactory proxyFactory;

        protected ResultReporter()
            : base(nameof(ResultReporter))
        {
            this.proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(
                    serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });
        }

        public static ResultReporter Create()
        {
            return new ResultReporter();
        }

        public void ReporAndForgetAsync(string reportingServiceUri, ReadOnlyCollection<OutputResult> results)
        {
            if (string.IsNullOrEmpty(reportingServiceUri) || results == null || results.Count <= 0)
            {
                return;
            }

            TaskHelper.FireAndForget(() => this.ReportAsync(reportingServiceUri, results), ex => this.UnhandledException(ex, nameof(this.ReporAndForgetAsync)));
        }

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ReportAsync(string reportingServiceUri, ReadOnlyCollection<OutputResult> results)
        {
            var client = this.proxyFactory.CreateServiceProxy<IReportingService>(new Uri(reportingServiceUri));
            await client.ReportDispatcherResultsAsync(results);
        }

        private void UnhandledException(Exception ex, [CallerMemberName] string methodName = "")
        {
            // TODO: shall we retry?
            MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, methodName, OperationStates.Failed, string.Empty, ex);
        }
    }
}
