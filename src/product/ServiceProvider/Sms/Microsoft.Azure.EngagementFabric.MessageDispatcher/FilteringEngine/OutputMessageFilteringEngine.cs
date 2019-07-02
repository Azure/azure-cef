// <copyright file="OutputMessageFilteringEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.FilteringEngine
{
    public class OutputMessageFilteringEngine : BaseComponent, IMessageFilteringEngine<OutputMessage>
    {
        private readonly DispatcherQueueSetting setting;
        private readonly IMessageFilteringEngine<InputMessage> inputMessageFilteringEngine;
        private readonly IResultReporter resultReporter;

        protected OutputMessageFilteringEngine(
            DispatcherQueueSetting setting,
            IMessageFilteringEngine<InputMessage> inputMessageFilteringEngine,
            IResultReporter resultReporter)
            : base(nameof(OutputMessageFilteringEngine))
        {
            this.setting = setting;
            this.inputMessageFilteringEngine = inputMessageFilteringEngine;
            this.resultReporter = resultReporter;
        }

        public static OutputMessageFilteringEngine Create(
            DispatcherQueueSetting dispatcherQueueSetting,
            IMessageFilteringEngine<InputMessage> inputMessageFilteringEngine,
            IResultReporter resultReporter)
        {
            return new OutputMessageFilteringEngine(
                dispatcherQueueSetting,
                inputMessageFilteringEngine,
                resultReporter);
        }

        public async Task<IList<OutputMessage>> FilterAsync(IReadOnlyList<OutputMessage> messages, CancellationToken cancellationToken)
        {
            if (messages == null || messages.Count <= 0)
            {
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(FilterAsync), OperationStates.Skipping, string.Empty);
                return null;
            }

            var outputMessages = new List<OutputMessage>();
            var droppedMessages = new List<OutputMessage>();

            var now = DateTime.UtcNow;
            var filteredMessages = new List<OutputMessage>();
            var messagesNeedingRefiltering = new List<InputMessage>();

            try
            {
                foreach (var message in messages)
                {
                    try
                    {
                        var expiration = message.MessageInfo.SendTime + this.setting.PartitionSetting.ServiceConfigureSetting.EventTimeToLive;
                        if (expiration < now)
                        {
                            MessageDispatcherEventSource.Current.Info(message.MessageInfo.TrackingId, this, nameof(this.FilterAsync), OperationStates.Dropped, $"msgId={message.MessageInfo.MessageId}, sendTime={message.MessageInfo.SendTime}, expired={true}, expiration={expiration}, timeLived={now - message.MessageInfo.SendTime}");
                            message.State = OutputMessageState.TimeOut;
                            droppedMessages.Add(message);
                            continue;
                        }

                        var msg = $"msgId={message.MessageInfo.MessageId}, batchId={message.Id}, outputMessageState={message.State}";
                        switch (message.State)
                        {
                            case OutputMessageState.Unfilterable:
                            case OutputMessageState.Unknown:
                                MessageDispatcherEventSource.Current.Info(message.MessageInfo.TrackingId, this, nameof(this.FilterAsync), OperationStates.Dropped, msg);
                                break;

                            case OutputMessageState.Nonfiltered:
                                messagesNeedingRefiltering.Add(new InputMessage(message));
                                MessageDispatcherEventSource.Current.Info(message.MessageInfo.TrackingId, this, nameof(this.FilterAsync), OperationStates.Starting, msg);
                                break;

                            case OutputMessageState.FilteredFailingDelivery:
                            case OutputMessageState.Filtered:
                                filteredMessages.Add(message);
                                break;

                            default:
                                throw new InvalidOperationException($"Unrecognized outputMessageState={message.State}");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageDispatcherEventSource.Current.Warning(message.MessageInfo.TrackingId, this, nameof(FilterAsync), OperationStates.Failed, $"msgId={message.MessageInfo.MessageId}, batchId={message.Id}, ex={ex.ToString()}");
                    }
                }

                if (messagesNeedingRefiltering.Count > 0)
                {
                    var refilteredMessages = await this.inputMessageFilteringEngine.FilterAsync(messagesNeedingRefiltering, cancellationToken);
                    filteredMessages.AddRange(refilteredMessages ?? new List<OutputMessage>(0));
                }
            }
            catch (Exception ex)
            {
                MessageDispatcherEventSource.Current.Warning(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(FilterAsync), OperationStates.Failed, ex.ToString());
            }

            if (droppedMessages.Count > 0)
            {
                var groups = droppedMessages.GroupBy(m => m.ReportingServiceUri);
                foreach (var group in groups)
                {
                    this.resultReporter.ReporAndForgetAsync(group.Key, group.Select(m => new OutputResult(m, new DeliveryResponse(RequestOutcome.TIMEOUT))).ToList().AsReadOnly());
                }
            }

            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.FilterAsync), OperationStates.Succeeded, $"NumInputs={messages.Count} NumOutput={outputMessages.Count} NmeDropped={droppedMessages.Count}");
            return filteredMessages;
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
