// <copyright file="MessageDispatcher.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Storage;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public class MessageDispatcher : RunAsyncComponent, IMessageDispatcher
    {
        private static readonly int MaxPostponeRetryCount = 10;
        private static readonly TimeSpan RetryErrorDelay = TimeSpan.FromSeconds(1);

        private readonly DispatcherQueueSetting setting;
        private readonly IReliableLog<OutputMessage> outputMessageQueue;
        private readonly IResultReporter resultReporter;
        private readonly ComponentManager dispatcherComponents;
        private readonly List<IPushWorker> pushWorkers;
        private readonly ThreadSafeRandom random;

        protected MessageDispatcher(
            DispatcherQueueSetting setting,
            IReliableLog<OutputMessage> outputMessageQueue,
            IResultReporter resultReporter)
            : base(nameof(MessageDispatcher))
        {
            this.setting = setting;
            this.outputMessageQueue = outputMessageQueue;
            this.resultReporter = resultReporter;
            this.pushWorkers = new List<IPushWorker>();
            this.random = new ThreadSafeRandom();

            this.dispatcherComponents = new ComponentManager(this.setting.Name, "MessageDispatcherContainer");
            this.dispatcherComponents.Faulted += (s, e) => this.Fault(e.Exception);

            for (var i = 0; i < this.setting.PushWorkerCount; ++i)
            {
                var pushWorker = PushWorker.Create(
                    this.setting,
                    this.outputMessageQueue,
                    this,
                    this.resultReporter);

                this.pushWorkers.Add(pushWorker);
                this.dispatcherComponents.Add(pushWorker);
            }
        }

        public static IMessageDispatcher Create(
            DispatcherQueueSetting setting,
            IReliableLog<OutputMessage> outputMessageQueue,
            IResultReporter resultReporter)
        {
            return new MessageDispatcher(
                setting,
                outputMessageQueue,
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

        public async Task<Task> DispatchAsync(IList<OutputMessage> outputMessages)
        {
            var operation = nameof(this.DispatchAsync);
            var queueLength = this.pushWorkers.Sum((t) => t.QueueLength);
            if (queueLength > this.setting.PartitionSetting.MaxPushQueueLength)
            {
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".Wait", OperationStates.Starting, $"while queueLength: {queueLength} le maxQueueLength: {this.setting.PartitionSetting.MaxPushQueueLength}");

                do
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), this.CancelToken);
                    queueLength = this.pushWorkers.Sum((t) => t.QueueLength);
                }
                while (queueLength > this.setting.PartitionSetting.MaxPushQueueLength);

                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, operation + ".Wait", OperationStates.Succeeded, $"queueLength: {queueLength} le maxQueueLength: {this.setting.PartitionSetting.MaxPushQueueLength}");
            }

            var pushWorkerIndex = this.random.Next(0, this.setting.PushWorkerCount);
            var tasks = new Task[outputMessages.Count];
            var batches = this.Split(outputMessages, this.setting.PushWorkerCount, tasks);
            for (var index = 0; index < batches.Length && !this.CancelToken.IsCancellationRequested; index++)
            {
                var batch = batches[index];
                if (batch.Count > 0)
                {
                    var pushWorker = this.pushWorkers[pushWorkerIndex];
                    if (!pushWorker.TryAdd(batch))
                    {
                        // False if cancelled
                        break;
                    }

                    pushWorkerIndex++;
                }

                if (pushWorkerIndex == this.setting.PushWorkerCount)
                {
                    pushWorkerIndex = 0;
                }
            }

            if (this.CancelToken.IsCancellationRequested)
            {
                for (var batchIndex = 0; batchIndex < batches.Length; batchIndex++)
                {
                    List<PushTaskInfo> batch = batches[batchIndex];
                    for (var taskIndex = 0; taskIndex < batch.Count; taskIndex++)
                    {
                        PushTaskInfo taskInfo = batch[taskIndex];
                        taskInfo.Tcs.TrySetCanceled();
                    }
                }
            }

            return Task.WhenAll(tasks);
        }

        public async Task PostponedAsync(List<PushTaskInfo> taskInfos)
        {
            for (var i = 0; i < taskInfos.Count; i++)
            {
                var outputMessage = taskInfos[i].OutputMessage;
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.PostponedAsync), OperationStates.Starting, $"msgId={outputMessage.MessageInfo.MessageId}, batchId={outputMessage.Id}, state={outputMessage.State}, deliveryCount={outputMessage.DeliveryCount}, delivered={outputMessage.Delivered}, requestExpiration={outputMessage.RequestExpiration}, newDeliveryTime={outputMessage.DeliveryTime}, destinationQueue={this.outputMessageQueue.Setting.Name}");
            }

            var attemptsCount = 1;
            while (!this.CancelToken.IsCancellationRequested && attemptsCount <= MaxPostponeRetryCount)
            {
                try
                {
                    var messages = taskInfos.Select(t => t.OutputMessage).ToList();
                    await this.outputMessageQueue.AppendAsync(messages);

                    for (var i = 0; i < taskInfos.Count; i++)
                    {
                        var taskInfo = taskInfos[i];
                        taskInfo.Tcs.TrySetResult(true);

                        var outputMessage = taskInfo.OutputMessage;
                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.PostponedAsync), OperationStates.Succeeded, $"msgId={outputMessage.MessageInfo.MessageId}, batchId={outputMessage.Id}, state={outputMessage.State}, deliveryCount={outputMessage.DeliveryCount}, delivered={outputMessage.Delivered}, requestExpiration={outputMessage.RequestExpiration}, newDeliveryTime={outputMessage.DeliveryTime}, destinationQueue={this.outputMessageQueue.Setting.Name}");
                    }

                    break;
                }
                catch (Exception ex)
                {
                    var retryDelay = RetryErrorDelay;
                    if (ex.IsFabricCriticalException())
                    {
                        retryDelay = TimeSpan.Zero;
                    }

                    for (var i = 0; i < taskInfos.Count; i++)
                    {
                        var outputMessage = taskInfos[i].OutputMessage;
                        MessageDispatcherEventSource.Current.Error(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(PostponedAsync), OperationStates.Failed, $"msgId={outputMessage.MessageInfo.MessageId}, batchId={outputMessage.Id}, state={outputMessage.State}, deliveryCount={outputMessage.DeliveryCount}, delivered={outputMessage.Delivered}, requestExpiration={outputMessage.RequestExpiration}, newDeliveryTime={outputMessage.DeliveryTime}, retryDelay={retryDelay}, attempt={attemptsCount}, ex={ex}");
                    }

                    if (ex.IsFabricCriticalException())
                    {
                        throw;
                    }

                    await TaskHelper.TryDelay(retryDelay, this.CancelToken);
                }

                attemptsCount++;
            }

            if (attemptsCount > MaxPostponeRetryCount)
            {
                for (var i = 0; i < taskInfos.Count; i++)
                {
                    var outputMessage = taskInfos[i].OutputMessage;
                    MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.PostponedAsync), OperationStates.Dropped, $"msgId={outputMessage.MessageInfo.MessageId}, batchId={outputMessage.Id}, state={outputMessage.State}, deliveryCount={outputMessage.DeliveryCount}, delivered={outputMessage.Delivered}, requestExpiration={outputMessage.RequestExpiration}, newDeliveryTime={outputMessage.DeliveryTime}, destinationQueue={this.outputMessageQueue.Setting.Name}");
                }
            }
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            await this.dispatcherComponents.OpenAsync(cancellationToken);
            await base.OnOpenAsync(cancellationToken);
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            await base.OnCloseAsync(cancellationToken);
            await this.dispatcherComponents.CloseAsync(cancellationToken);
        }

        private List<PushTaskInfo>[] Split(IList<OutputMessage> outputMessages, int batchCount, Task[] tasks)
        {
            var batches = new List<PushTaskInfo>[Math.Min(batchCount, outputMessages.Count)];
            for (var i = 0; i < batches.Length; i++)
            {
                batches[i] = new List<PushTaskInfo>();
            }

            var batchIndex = 0;
            var taskIndex = 0;
            var now = DateTime.UtcNow;
            var postponed = new List<PushTaskInfo>();

            for (var i = 0; i < outputMessages.Count; i++)
            {
                var batch = batches[batchIndex];
                var outputMessage = outputMessages[i];
                var pushTaskInfo = new PushTaskInfo()
                {
                    OutputMessage = outputMessage,
                    Tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously),
                    PushEnqueueTimestamp = DateTime.UtcNow
                };

                var expiration = outputMessage.MessageInfo.SendTime + this.setting.PartitionSetting.ServiceConfigureSetting.EventTimeToLive;
                bool expired = expiration < now;
                if (expired || outputMessage.Delivered || outputMessage.State == OutputMessageState.Unfilterable)
                {
                    MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.Split), OperationStates.Dropped, $"msgId={outputMessage.MessageInfo.MessageId}, batchId={outputMessage.Id}, state={outputMessage.State}, deliveryCount={outputMessage.DeliveryCount}, delivered={outputMessage.Delivered}, requestExpiration={outputMessage.RequestExpiration}, publishTime={outputMessage.MessageInfo.SendTime}, expired={expired}, expiration={expiration}, timeLived={now - outputMessage.MessageInfo.SendTime}");
                    pushTaskInfo.Tcs.TrySetResult(true);
                }
                else if (outputMessage.State == OutputMessageState.Filtered)
                {
                    batch.Add(pushTaskInfo);
                    batchIndex++;
                }
                else
                {
                    pushTaskInfo.OutputMessage.DeliveryTime = DateTime.UtcNow.Add(this.outputMessageQueue.Setting.RetryDelay);
                    postponed.Add(pushTaskInfo);
                }

                tasks[taskIndex] = pushTaskInfo.Tcs.Task;

                taskIndex++;
                if (batchIndex == batchCount)
                {
                    batchIndex = 0;
                }
            }

            if (postponed.Count > 0)
            {
                TaskHelper.FireAndForget(() => this.PostponedAsync(postponed), ex => this.UnhandledException(ex, nameof(this.PostponedAsync)));
            }

            return batches;
        }

        private void UnhandledException(Exception ex, [CallerMemberName] string methodName = "")
        {
            if (!(ex is OperationCanceledException))
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, methodName, OperationStates.Faulting, string.Empty, ex);
                this.Fault(ex);
            }
        }
    }
}
