// <copyright file="InputMessageFilteringEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.FilteringEngine
{
    public class InputMessageFilteringEngine : BaseComponent, IMessageFilteringEngine<InputMessage>
    {
        private static readonly Uri MetaServiceUri = new Uri("fabric:/SmsApp/SmsMetaService");
        private readonly DispatcherQueueSetting setting;
        private readonly IResultReporter resultReporter;

        private ServiceProxyFactory proxyFactory;

        protected InputMessageFilteringEngine(
            DispatcherQueueSetting setting,
            IResultReporter resultReporter)
            : base(nameof(InputMessageFilteringEngine))
        {
            this.setting = setting;
            this.resultReporter = resultReporter;
            this.proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(
                    serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });
        }

        public static InputMessageFilteringEngine Create(
            DispatcherQueueSetting dispatcherQueueSetting,
            IResultReporter resultReporter)
        {
            return new InputMessageFilteringEngine(
                dispatcherQueueSetting,
                resultReporter);
        }

        public Task<IList<OutputMessage>> FilterAsync(IReadOnlyList<InputMessage> messages, CancellationToken cancellationToken)
        {
            if (messages == null || messages.Count <= 0)
            {
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(FilterAsync), OperationStates.Skipping, string.Empty);
                return null;
            }

            var outputMessages = new List<OutputMessage>();
            var droppedMessages = new List<OutputMessage>();

            var numUnmatchedInputMessages = 0;
            var numFailedInputMessages = 0;
            var now = DateTime.UtcNow;
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            foreach (var message in messages)
            {
                try
                {
                    // Validate message info
                    if (message.MessageInfo == null)
                    {
                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.FilterAsync), OperationStates.Dropped, $"Input message dropped because of invalid message info.");
                        var outputMessage = new OutputMessage(message, message.Targets, OutputMessageState.Unfilterable);
                        droppedMessages.Add(outputMessage);
                        continue;
                    }

                    // Validate targets
                    if (message.Targets == null || message.Targets.Count <= 0)
                    {
                        MessageDispatcherEventSource.Current.Info(message.MessageInfo.TrackingId, this, nameof(this.FilterAsync), OperationStates.Dropped, $"Input message dropped because of no target. msgId={message.MessageInfo.MessageId}, sendTime={message.MessageInfo.SendTime}");
                        var outputMessage = new OutputMessage(message, message.Targets, OutputMessageState.Unfilterable);
                        droppedMessages.Add(outputMessage);
                        continue;
                    }

                    // Validate connector info
                    if (!message.ConnectorCredential.IsValid())
                    {
                        MessageDispatcherEventSource.Current.Info(message.MessageInfo.TrackingId, this, nameof(this.FilterAsync), OperationStates.Dropped, $"Input message dropped because of invalid connector info. msgId={message.MessageInfo.MessageId}, sendTime={message.MessageInfo.SendTime} connectorInfo={message.ConnectorCredential}");
                        var outputMessage = new OutputMessage(message, message.Targets, OutputMessageState.Unfilterable);
                        droppedMessages.Add(outputMessage);
                        continue;
                    }

                    // Validate expiration time
                    var expiration = message.MessageInfo.SendTime + this.setting.PartitionSetting.ServiceConfigureSetting.EventTimeToLive;
                    if (expiration < now)
                    {
                        MessageDispatcherEventSource.Current.Info(message.MessageInfo.TrackingId, this, nameof(this.FilterAsync), OperationStates.Dropped, $"Input message dropped because of expiration. msgId={message.MessageInfo.MessageId}, sendTime={message.MessageInfo.SendTime}, expired={true}, expiration={expiration}, timeLived={now - message.MessageInfo.SendTime}");
                        var outputMessage = new OutputMessage(message, message.Targets, OutputMessageState.TimeOut);
                        droppedMessages.Add(outputMessage);
                        continue;
                    }

                    var batches = message.Targets
                        .Select((x, i) => new { Index = i, value = x })
                        .GroupBy(x => x.Index / message.ConnectorCredential.BatchSize)
                        .Select(x => x.Select(v => v.value).ToList())
                        .ToList();

                    foreach (var batch in batches)
                    {
                        var output = new OutputMessage(message, batch.AsReadOnly(), OutputMessageState.Filtered);
                        outputMessages.Add(output);
                    }
                }
                catch (Exception ex)
                {
                    MessageDispatcherEventSource.Current.Warning(message.MessageInfo?.TrackingId, this, nameof(FilterAsync), OperationStates.Failed, $"msgId={message.MessageInfo.MessageId} ex={ex.ToString()}");

                    var output = new OutputMessage(message, message.Targets, OutputMessageState.Nonfiltered);
                    outputMessages.Add(output);
                    numFailedInputMessages++;
                }
            }

            if (droppedMessages.Count > 0)
            {
                var groups = droppedMessages.GroupBy(m => m.ReportingServiceUri);
                foreach (var group in groups)
                {
                    this.resultReporter.ReporAndForgetAsync(group.Key, group.Select(m => new OutputResult(m, new DeliveryResponse(m.State == OutputMessageState.TimeOut ? RequestOutcome.TIMEOUT : RequestOutcome.UNKNOWN))).ToList().AsReadOnly());
                }
            }

            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.FilterAsync), OperationStates.Succeeded, $"{stopwatch.ElapsedMilliseconds} ms NumInputs={messages.Count} NumUnmatchedInput={numUnmatchedInputMessages} NumFailedInput={numFailedInputMessages} NumOutput={outputMessages.Count} NmeDropped={droppedMessages.Count}");
            return Task.FromResult<IList<OutputMessage>>(outputMessages);
        }

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
