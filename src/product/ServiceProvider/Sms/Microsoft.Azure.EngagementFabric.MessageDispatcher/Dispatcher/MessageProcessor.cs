// <copyright file="MessageProcessor.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.FilteringEngine;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Storage;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public class MessageProcessor<TMessage> : CancelTokenComponent, IMessageProcessor<TMessage>
    {
        private readonly DispatcherQueueSetting setting;
        private readonly IReliableLog<TMessage> inputMessageQueue;
        private readonly IReliableLog<OutputMessage> outputMessageQueue;
        private readonly ComponentManager processorComponents;
        private readonly IMessagePump messagePump;
        private readonly IMessageDispatcher messageDispatcher;
        private readonly IMessageFilteringEngine<TMessage> filteringEngine;
        private readonly IResultReporter resultReporter;

        protected MessageProcessor(
            DispatcherQueueSetting setting,
            IReliableLog<TMessage> inputMessageQueue,
            IReliableLog<OutputMessage> outputMessageQueue,
            IMessageFilteringEngine<TMessage> filteringEngine,
            IResultReporter resultReporter)
            : base(nameof(MessageProcessor<TMessage>))
        {
            this.setting = setting;
            this.inputMessageQueue = inputMessageQueue;
            this.outputMessageQueue = outputMessageQueue;
            this.filteringEngine = filteringEngine;
            this.resultReporter = resultReporter;

            this.processorComponents = new ComponentManager(this.setting.Name, "MessageProcessorContainer");
            this.processorComponents.Faulted += (s, e) => this.Fault(e.Exception);

            this.messageDispatcher = MessageDispatcher.Create(
                this.setting,
                this.outputMessageQueue,
                this.resultReporter);
            this.processorComponents.Add(this.messageDispatcher);

            this.messagePump = MessagePump<TMessage>.Create(
                this.setting,
                this.inputMessageQueue,
                this.filteringEngine,
                this.messageDispatcher);
            this.processorComponents.Add(this.messagePump);
        }

        public static IMessageProcessor<TMessage> Create(
            DispatcherQueueSetting setting,
            IReliableLog<TMessage> inputMessageQueue,
            IReliableLog<OutputMessage> outputMessageQueue,
            IMessageFilteringEngine<TMessage> filteringEngine,
            IResultReporter resultReporter)
        {
            return new MessageProcessor<TMessage>(
                setting,
                inputMessageQueue,
                outputMessageQueue,
                filteringEngine,
                resultReporter);
        }

        public override string ToString()
        {
            return TracingHelper.FormatTraceSource(this, this.setting.Name);
        }

        public override string GetTraceState()
        {
            return $"Component={this.Component} Queue={this.setting.Name}";
        }

        public async Task AppendAsync(IReadOnlyList<TMessage> events)
        {
            await this.inputMessageQueue.AppendAsync(events);
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            await this.processorComponents.OpenAsync(cancellationToken);
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            await this.processorComponents.CloseAsync(cancellationToken);
        }
    }
}
