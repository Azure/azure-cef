// <copyright file="MessageDispatcher.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.FilteringEngine;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Storage;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class MessageDispatcher : StatefulService, IDispatcherService
    {
        private IReliableLog<InputMessage> inputReliableLog;
        private IResultReporter resultReporter;
        private ComponentManager components;

        public MessageDispatcher(StatefulServiceContext context)
            : base(context)
        {
            this.components = new ComponentManager(nameof(MessageDispatcher), "RootContainer");

            var partitionSetting = DispatcherPartitionSetting.Create();
            var resultReporter = ResultReporter.Create();
            var inputMessageFilteringEngine = InputMessageFilteringEngine.Create(partitionSetting.InstantQueueSetting, resultReporter);

            // Delayed Queue
            IReliableLog<OutputMessage> nextOutputReliableLog = null;
            for (var i = partitionSetting.DelayedQueueSettings.Count - 1; i >= 0; i--)
            {
                var setting = partitionSetting.DelayedQueueSettings[i];
                var outputMessageFilteringEngine = OutputMessageFilteringEngine.Create(setting, inputMessageFilteringEngine, resultReporter);
                var outputReliableLog = ReliableLog<OutputMessage>.Create(setting, this.StateManager);
                outputReliableLog = new DelayedReliableLog(outputReliableLog);
                var outputMessageProcessor = MessageProcessor<OutputMessage>.Create(
                    setting,
                    outputReliableLog,
                    nextOutputReliableLog ?? outputReliableLog,
                    outputMessageFilteringEngine,
                    resultReporter);

                this.components.Add(outputMessageFilteringEngine);
                this.components.Add(outputReliableLog);
                this.components.Add(outputMessageProcessor);

                nextOutputReliableLog = outputReliableLog;
            }

            // Instant Queue
            var inputReliableLog = ReliableLog<InputMessage>.Create(partitionSetting.InstantQueueSetting, this.StateManager);
            var inputMessageProcessor = MessageProcessor<InputMessage>.Create(
                partitionSetting.InstantQueueSetting,
                inputReliableLog,
                nextOutputReliableLog,
                inputMessageFilteringEngine,
                resultReporter);

            this.components.Add(inputMessageFilteringEngine);
            this.components.Add(inputReliableLog);
            this.components.Add(inputMessageProcessor);
            this.components.Add(resultReporter);

            this.inputReliableLog = inputReliableLog;
            this.resultReporter = resultReporter;

            MessageDispatcher.AppName = context.CodePackageActivationContext.ApplicationTypeName;
        }

        public static string AppName { get; private set; }

        public async Task DispatchAsync(List<InputMessage> messages, CancellationToken cancellationToken)
        {
            await this.inputReliableLog.AppendAsync(messages);
        }

        /// <summary>
        /// Optional override to create listeners (e.g., HTTP, Service Remoting, WCF, etc.) for this service replica to handle client or user requests.
        /// </summary>
        /// <remarks>
        /// For more information on service communication, see https://aka.ms/servicefabricservicecommunication
        /// </remarks>
        /// <returns>A collection of listeners.</returns>
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener((c) =>
                {
                    return new FabricTransportServiceRemotingListener(c, this, null, new ServiceRemotingJsonSerializationProvider());
                })
            };
        }

        /// <summary>
        /// This is the main entry point for your service replica.
        /// This method executes when this replica of your service becomes primary and has write status.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service replica.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            try
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"{MessageDispatcher.AppName}-MessageDispatcher service is starting.");

                var cancelRunCompletionSource = new TaskCompletionSource<bool>();
                cancellationToken.Register(() =>
                {
                    cancelRunCompletionSource.TrySetResult(true);
                });

                await components.OpenAsync(cancellationToken);
                await cancelRunCompletionSource.Task;
            }
            catch (Exception exception)
            {
                ServiceEventSource.Current.ServiceMessage(this.Context, $"{MessageDispatcher.AppName}-MessageDispatcher service is getting exception: {exception.ToString()}.");

                this.Partition.ReportFault(FaultType.Permanent);
            }
            finally
            {
                try
                {
                    using (var cancelSource = new CancellationTokenSource(TimeSpan.FromSeconds(15)))
                    {
                        await this.components.CloseAsync(cancelSource.Token);
                    }
                }
                catch (Exception exception)
                {
                    ServiceEventSource.Current.ServiceMessage(this.Context, $"{MessageDispatcher.AppName}-MessageDispatcher service is getting exception during closing: {exception.ToString()}.");
                }

                ServiceEventSource.Current.ServiceMessage(this.Context, $"{MessageDispatcher.AppName}-MessageDispatcher service is closed");
            }
        }
    }
}
