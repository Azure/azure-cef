// <copyright file="MessagePump.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.FilteringEngine;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Storage;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public class MessagePump<TMessage> : RunAsyncComponent, IMessagePump
    {
        private static readonly TimeSpan CheckpointDelay = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan CheckpointErrorDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan ReadTimeout = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan UnhandledExceptionDelay = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan MinimumRunDurationBeforeFaulting = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan FabricExceptionsDurationBeforeFaulting = TimeSpan.FromMinutes(5);

        private readonly DispatcherQueueSetting setting;
        private readonly IReliableLog<TMessage> messageQueue;
        private readonly IMessageFilteringEngine<TMessage> filteringEngine;
        private readonly IMessageDispatcher messageDispatcher;
        private readonly ConcurrentQueue<CheckpointInfo> checkpointQueue;

        private bool inclusive;

        protected MessagePump(
            DispatcherQueueSetting setting,
            IReliableLog<TMessage> messageQueue,
            IMessageFilteringEngine<TMessage> filteringEngine,
            IMessageDispatcher messageDispatcher)
            : base(nameof(MessagePump<TMessage>))
        {
            this.setting = setting;
            this.messageQueue = messageQueue;
            this.filteringEngine = filteringEngine;
            this.messageDispatcher = messageDispatcher;
            this.checkpointQueue = new ConcurrentQueue<CheckpointInfo>();
        }

        public static IMessagePump Create(
            DispatcherQueueSetting setting,
            IReliableLog<TMessage> messageQueue,
            IMessageFilteringEngine<TMessage> filteringEngine,
            IMessageDispatcher messageDispatcher)
        {
            return new MessagePump<TMessage>(
               setting,
               messageQueue,
               filteringEngine,
               messageDispatcher);
        }

        public override string ToString()
        {
            return TracingHelper.FormatTraceSource(this, this.setting.Name);
        }

        public override string GetTraceState()
        {
            return $"Component={this.Component} Queue={this.setting.Name}";
        }

        protected override Func<CancellationToken, Task>[] OnRunMultiple()
        {
            return new Func<CancellationToken, Task>[]
            {
                this.ProcessMessagesRunAsync,
                this.CheckpointRunAsync
            };
        }

        private async Task ProcessMessagesRunAsync(CancellationToken cancelToken)
        {
            var operation = nameof(this.ProcessMessagesRunAsync);
            var runStartTimestamp = DateTime.UtcNow;

            var offset = await this.messageQueue.GetCheckpointedRecordInfoAsync();
            if (offset.Index == RecordInfo.InvalidIndex)
            {
                offset = offset.Next();
                this.inclusive = true;
            }
            else
            {
                this.inclusive = false;
            }

            var fabricErrorsStartedTimestamp = DateTime.Now;
            var fabricErrorsStarted = false;

            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".Read", OperationStates.Starting, $"requestOffset: {offset}, inclusive: {inclusive}");
            while (!cancelToken.IsCancellationRequested)
            {
                var pumpStartTimestamp = DateTime.UtcNow;

                try
                {
                    IReadOnlyList<Record<TMessage>> readResult = null;

                    readResult = await this.messageQueue.ReadAsync(
                        offset,
                        this.setting.PartitionSetting.MessagePumpBatchSize,
                        this.inclusive,
                        ReadTimeout,
                        cancelToken);

                    if (readResult == null || readResult.Count == 0)
                    {
                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".Read", OperationStates.Succeeded, $"requestOffset: {offset}, inclusive: {inclusive}, resultOffset: NULL, resultMessageCount: 0");
                    }
                    else
                    {
                        var inputMessages = readResult.Select(r => r.Item).ToList();

                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".Read", OperationStates.Succeeded, $"requestOffset: {offset}, inclusive: {inclusive}, resultOffset: {readResult[readResult.Count - 1].RecordInfo.Index}, resultMessageCount: {inputMessages.Count}");

                        if (this.inclusive == true)
                        {
                            this.inclusive = false;
                        }

                        IList<OutputMessage> filteredEvents;

                        try
                        {
                            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".Filter", OperationStates.Starting, $"inputEventCount: {inputMessages.Count}");
                            filteredEvents = await this.filteringEngine.FilterAsync(inputMessages, cancelToken);
                            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".Filter", OperationStates.Succeeded, $"outputEventCount: {filteredEvents.Count}");
                        }
                        catch (Exception ex)
                        {
                            MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".Filter", OperationStates.Failed, string.Empty, ex);
                            throw;
                        }

                        var checkpointInfo = new CheckpointInfo(readResult[0].RecordInfo, readResult[readResult.Count - 1].RecordInfo, inputMessages.Count);
                        this.checkpointQueue.Enqueue(checkpointInfo);

                        if (filteredEvents != null && filteredEvents.Count > 0)
                        {
                            await this.ScheduleDeliveryAsync(filteredEvents, checkpointInfo);
                        }
                        else
                        {
                            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".ScheduleDelivery", OperationStates.Skipping, string.Empty);
                            checkpointInfo.Tcs.SetResult(true);
                        }

                        offset = checkpointInfo.LastRecordInfo;
                    }

                    fabricErrorsStarted = false;
                }
                catch (Exception ex)
                {
                    var runStartElapsed = runStartTimestamp - DateTime.UtcNow;
                    var pumpStartElapsed = pumpStartTimestamp - DateTime.UtcNow;

                    if (runStartElapsed < MinimumRunDurationBeforeFaulting || !ex.IsFabricCriticalException())
                    {
                        fabricErrorsStarted = false;
                        MessageDispatcherEventSource.Current.Warning(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.Failed, $"runStartElapsed={runStartElapsed} pumpStartElapsed={pumpStartElapsed} Waiting for={UnhandledExceptionDelay.TotalMilliseconds} ms Ex={ex.ToString()}");
                        await TaskHelper.TryDelay(UnhandledExceptionDelay, cancelToken);
                    }
                    else
                    {
                        var fabricErrorsStartedElapsed = fabricErrorsStartedTimestamp - DateTime.UtcNow;
                        var faultMessage = $"MessagePump encountered a critical fabric exception. FabricErrorsStartedElapsed={fabricErrorsStartedElapsed} (Threshold={FabricExceptionsDurationBeforeFaulting}) AND runStartElapsed={runStartElapsed} (Threshold={MinimumRunDurationBeforeFaulting}) pumpStartElapsed={pumpStartElapsed}";

                        if (!fabricErrorsStarted)
                        {
                            fabricErrorsStarted = true;
                            fabricErrorsStartedTimestamp = DateTime.UtcNow;
                            MessageDispatcherEventSource.Current.Warning(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.FailedNotFaulting, faultMessage + $" ex={ex.ToString()}");
                        }
                        else
                        {
                            if (runStartElapsed > MinimumRunDurationBeforeFaulting &&
                                fabricErrorsStartedElapsed > FabricExceptionsDurationBeforeFaulting)
                            {
                                MessageDispatcherEventSource.Current.Error(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.Faulting, faultMessage + $" ex={ex.ToString()}");
                                this.FaultPump(faultMessage, ex);
                            }
                            else
                            {
                                MessageDispatcherEventSource.Current.Warning(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.FailedNotFaulting, faultMessage + $" ex={ex.ToString()}");
                            }
                        }
                    }
                }
            }

            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.Succeeded, $"requestOffset: {offset}, inclusive: {inclusive}");
        }

        private async Task ScheduleDeliveryAsync(IList<OutputMessage> outputMessages, CheckpointInfo checkpointInfo)
        {
            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.ScheduleDeliveryAsync), OperationStates.Starting, $"outputMessageCount: {outputMessages.Count}, checkpointInfo: {checkpointInfo}");
            var deliverTask = await this.messageDispatcher.DispatchAsync(outputMessages).ConfigureAwait(false);
            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.ScheduleDeliveryAsync), OperationStates.Succeeded, $"outputMessageCount: {outputMessages.Count}, checkpointInfo: {checkpointInfo}");

            this.CompleteScheduleDeliveryAsync(deliverTask, checkpointInfo).Fork();
        }

        private async Task CompleteScheduleDeliveryAsync(Task deliverTask, CheckpointInfo checkpointInfo)
        {
            // Wait for outoging messages to be either pushed or moved to next queues before doing checkpoint
            try
            {
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CompleteScheduleDeliveryAsync), OperationStates.Starting, $"checkpointInfo: {checkpointInfo}");
                await deliverTask;
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CompleteScheduleDeliveryAsync), OperationStates.Succeeded, $"checkpointInfo: {checkpointInfo}");

                checkpointInfo.Tcs.SetResult(true);
            }
            catch (OperationCanceledException)
            {
                // Do nothing in this case
            }
            catch (Exception ex)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CompleteScheduleDeliveryAsync), OperationStates.Faulting, string.Empty, ex);
                this.FaultPump($"CompleteScheduleDeliveryAsync failed unexpectedly when completing.", ex);
            }
        }

        private async Task CheckpointRunAsync(CancellationToken cancelToken)
        {
            var operation = nameof(this.CheckpointRunAsync);
            try
            {
                CheckpointInfo checkpointInfo;
                do
                {
                    MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.Starting, $"checkpointQueueCount: {this.checkpointQueue.Count}");
                    checkpointInfo = await this.GetNextCheckpointInfoAsync(cancelToken);
                    while (!cancelToken.IsCancellationRequested)
                    {
                        try
                        {
                            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".InnerLoop", OperationStates.Starting, $"CheckpointInfo: {checkpointInfo}");
                            await this.messageQueue.CheckpointAsync(checkpointInfo);
                            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".InnerLoop", OperationStates.Succeeded, $"CheckpointInfo: {checkpointInfo}");
                            break;
                        }
                        catch (Exception ex)
                        {
                            checkpointInfo.Retries++;
                            var checkpointErrorDelay = checkpointInfo.Retries < 3 ? CheckpointErrorDelay : (checkpointInfo.Retries < 60 ? TimeSpan.FromSeconds(checkpointInfo.Retries) : TimeSpan.FromSeconds(60));
                            MessageDispatcherEventSource.Current.Error(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".InnerLoop", OperationStates.Failed, $"CheckpointInfo: {checkpointInfo}, nextAttemptAfter: {checkpointErrorDelay} Exception: {ex}");
                            if (checkpointInfo.Retries >= this.setting.MaximumPumpRetries)
                            {
                                this.FaultPump($"Checkpointing is not making progress. Current checkpoint info: {checkpointInfo}, checkpoint queue count: {this.checkpointQueue.Count}", ex);
                            }

                            await TaskHelper.TryDelay(checkpointErrorDelay, cancelToken);
                        }
                    }

                    MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.Succeeded, string.Empty);
                }
                while (checkpointInfo != null);
            }
            catch (Exception ex)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.Failed, string.Empty, ex);
                throw;
            }

            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation, OperationStates.Succeeded, string.Empty);
        }

        private async Task<CheckpointInfo> GetNextCheckpointInfoAsync(CancellationToken cancelToken)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                CheckpointInfo nextCheckpointInfo;
                while (this.checkpointQueue.TryDequeue(out nextCheckpointInfo))
                {
                    if (!nextCheckpointInfo.Tcs.Task.IsCompleted)
                    {
                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.GetNextCheckpointInfoAsync), OperationStates.Starting, $"CheckpointInfo: {nextCheckpointInfo}");
                        await nextCheckpointInfo.Tcs.Task;
                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.GetNextCheckpointInfoAsync), OperationStates.Succeeded, $"CheckpointInfo: {nextCheckpointInfo}");
                    }

                    // Collapse checkpoint info
                    CheckpointInfo ci;
                    while (this.checkpointQueue.TryPeek(out ci) &&
                        ci.Tcs.Task.IsCompleted)
                    {
                        ci.FirstRecordInfo = nextCheckpointInfo.FirstRecordInfo;
                        ci.RecordCount += nextCheckpointInfo.RecordCount;
                        this.checkpointQueue.TryDequeue(out nextCheckpointInfo);
                    }

                    return nextCheckpointInfo;
                }

                await TaskHelper.TryDelay(CheckpointDelay, cancelToken);
            }

            return null;
        }

        private void FaultPump(string exceptionMessage, Exception ex)
        {
            var message = $"{this.setting.Name}:{exceptionMessage}";
            this.Fault(new Exception(message, ex));
        }
    }
}
