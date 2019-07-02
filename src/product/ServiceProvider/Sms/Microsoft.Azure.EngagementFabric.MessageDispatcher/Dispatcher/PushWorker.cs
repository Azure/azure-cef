// <copyright file="PushWorker.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Storage;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public class PushWorker : RunAsyncComponent, IPushWorker
    {
        private static readonly TimeSpan SignalWaitTime = TimeSpan.FromHours(1);
        private static readonly TimeSpan PostTimeout = TimeSpan.FromSeconds(60);
        private static readonly TimeSpan PostExpirationTimeSpan = TimeSpan.FromMinutes(3);

        private readonly DispatcherQueueSetting setting;
        private readonly IReliableLog<OutputMessage> outputMessageQueue;
        private readonly IMessageDispatcher messageDispatcher;
        private readonly IResultReporter resultReporter;

        private readonly AsyncSignal pushTaskAvailableSignal;

        private ConcurrentQueue<PushTaskInfo> messageQueue;

        protected PushWorker(
            DispatcherQueueSetting setting,
            IReliableLog<OutputMessage> outputMessageQueue,
            IMessageDispatcher messageDispatcher,
            IResultReporter resultReporter)
            : base(nameof(PushWorker))
        {
            this.setting = setting;
            this.outputMessageQueue = outputMessageQueue;
            this.messageDispatcher = messageDispatcher;
            this.pushTaskAvailableSignal = new AsyncSignal(SignalWaitTime);
            this.messageQueue = new ConcurrentQueue<PushTaskInfo>();
            this.resultReporter = resultReporter;
        }

        public int QueueLength => this.messageQueue?.Count ?? 0;

        public static IPushWorker Create(
            DispatcherQueueSetting setting,
            IReliableLog<OutputMessage> outputMessageQueue,
            IMessageDispatcher messageDispatcher,
            IResultReporter resultReporter)
        {
            return new PushWorker(
                setting,
                outputMessageQueue,
                messageDispatcher,
                resultReporter);
        }

        public override string ToString()
        {
            return TracingHelper.FormatTraceSource(this, this.setting.Name);
        }

        public override string GetTraceState()
        {
            return $"Component={this.Component} Queue={this.setting.Name} QueueLength={this.QueueLength}";
        }

        public bool TryAdd(List<PushTaskInfo> tasks)
        {
            if (this.CancelToken.IsCancellationRequested)
            {
                return false;
            }

            foreach (var task in tasks)
            {
                this.messageQueue.Enqueue(task);
            }

            this.pushTaskAvailableSignal.Set();
            return true;
        }

        protected override async Task OnRunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (this.messageQueue.Count <= 0)
                {
                    await this.pushTaskAvailableSignal.WaitAsync().ConfigureAwait(false);
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                PushTaskInfo taskInfo;
                if (this.messageQueue.TryDequeue(out taskInfo) && taskInfo != null)
                {
                    TaskHelper.FireAndForget(() => this.StartDeliveryTask(taskInfo), ex => this.UnhandledException(ex, nameof(this.OnRunAsync)));
                }
            }

            while (this.messageQueue.Count > 0)
            {
                PushTaskInfo taskInfo;
                if (this.messageQueue.TryDequeue(out taskInfo) && taskInfo != null)
                {
                    taskInfo.Tcs.TrySetCanceled();
                }
            }
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.pushTaskAvailableSignal.Dispose();
        }

        private async Task StartDeliveryTask(PushTaskInfo taskInfo)
        {
            var operation = nameof(this.StartDeliveryTask);
            try
            {
                var response = new DeliveryResponse(RequestOutcome.UNKNOWN);
                var outputMessage = taskInfo.OutputMessage;
                var now = DateTime.UtcNow;
                var expiration = outputMessage.MessageInfo.SendTime + this.setting.PartitionSetting.ServiceConfigureSetting.EventTimeToLive;
                var expired = expiration < now;

                if (expired)
                {
                    MessageDispatcherEventSource.Current.Info(taskInfo.OutputMessage.MessageInfo.TrackingId, this, operation + ".Deliver", OperationStates.Dropped, $"msgId={outputMessage.MessageInfo.MessageId}, batchId={outputMessage.Id}, state={outputMessage.State}, deliveryCount={outputMessage.DeliveryCount}, delivered={outputMessage.Delivered}, requestExpiration={outputMessage.RequestExpiration}, SendTime={outputMessage.MessageInfo.SendTime}, expired={expired}, expiration={expiration}, timeLived={now - outputMessage.MessageInfo.SendTime}");
                    return;
                }

                if (outputMessage.State == OutputMessageState.Filtered || outputMessage.State == OutputMessageState.FilteredFailingDelivery)
                {
                    if (outputMessage.RequestExpiration > now)
                    {
                        response.DeliveryOutcome = RequestOutcome.DELIVERING;
                    }
                    else
                    {
                        var deliverTask = this.DeliverAsync(taskInfo, now);
                        if (await Task.WhenAny(deliverTask, Task.Delay(PostTimeout)) == deliverTask)
                        {
                            response = deliverTask.Result;
                        }
                        else
                        {
                            response.DeliveryOutcome = RequestOutcome.TIMEOUT;
                            MessageDispatcherEventSource.Current.Warning(taskInfo.OutputMessage.MessageInfo.TrackingId, this, operation + ".Deliver", OperationStates.TimedOut, $"msgId={outputMessage.MessageInfo.MessageId}, batchId={outputMessage.Id}, outcome={response.DeliveryOutcome}, delivered={outputMessage.Delivered}");
                        }
                    }
                }

                if (response.DeliveryOutcome != RequestOutcome.FAILED_UNKNOWN)
                {
                    this.resultReporter.ReporAndForgetAsync(outputMessage.ReportingServiceUri, new List<OutputResult> { new OutputResult(outputMessage, response) }.AsReadOnly());
                }

                // Only retry for unknown failure
                else
                {
                    if (outputMessage.DeliveryCount < this.setting.PartitionSetting.MaximumDeliveryCount)
                    {
                        outputMessage.DeliveryCount++;
                    }
                    else
                    {
                        MessageDispatcherEventSource.Current.Info(taskInfo.OutputMessage.MessageInfo.TrackingId, this, operation + ".Postpone", OperationStates.Dropped, $"msgId={outputMessage.MessageInfo.MessageId}, batchId={outputMessage.Id}, state={outputMessage.State}, deliveryCount={outputMessage.DeliveryCount}, outcome={response.DeliveryOutcome}, delivered={outputMessage.Delivered}, requestExpiration={outputMessage.RequestExpiration}, SendTime={outputMessage.MessageInfo.SendTime}, expired={expired}, expiration={expiration}, timeLived={now - outputMessage.MessageInfo.SendTime}");
                        this.resultReporter.ReporAndForgetAsync(outputMessage.ReportingServiceUri, new List<OutputResult> { new OutputResult(outputMessage, response) }.AsReadOnly());
                        return;
                    }

                    outputMessage.DeliveryTime = DateTime.UtcNow.Add(this.outputMessageQueue.Setting.RetryDelay);
                    TaskHelper.FireAndForget(() => this.messageDispatcher.PostponedAsync(new List<PushTaskInfo> { taskInfo }), ex => UnhandledException(ex, nameof(this.StartDeliveryTask)));
                }
            }
            catch (Exception ex)
            {
                MessageDispatcherEventSource.Current.Warning(taskInfo.OutputMessage.MessageInfo.TrackingId, this, operation, OperationStates.Failed, ex.ToString());
                taskInfo.Tcs.TrySetException(ex);
            }
            finally
            {
                taskInfo.Tcs.TrySetResult(true);
            }
        }

        private async Task<DeliveryResponse> DeliverAsync(PushTaskInfo taskInfo, DateTime now)
        {
            DeliveryRequest deliveryRequest = null;
            DeliveryResponse deliveryResponse = new DeliveryResponse(RequestOutcome.UNKNOWN);
            Exception exception = null;

            try
            {
                deliveryRequest = new DeliveryRequest(taskInfo.OutputMessage, now + PostExpirationTimeSpan);
                MessageDispatcherEventSource.Current.Info(taskInfo.OutputMessage.MessageInfo.TrackingId, this, nameof(this.DeliverAsync), OperationStates.Starting, $"msgId={taskInfo.OutputMessage.MessageInfo.MessageId}, batchId={taskInfo.OutputMessage.Id}, delivered={taskInfo.OutputMessage.Delivered}, requestExpiration={taskInfo.OutputMessage.RequestExpiration}");

                var pushConnector = PushConnector.Create(taskInfo.OutputMessage.ConnectorCredential);
                deliveryResponse = await pushConnector.DeliverAsync(deliveryRequest, this.CancelToken).ConfigureAwait(false);
            }
            catch (SocketException ex)
            {
                exception = ex;
                deliveryResponse.DeliveryOutcome = RequestOutcome.FAILED_OPERATOR;
                deliveryResponse.DeliveryDetail = ex.Message;
            }
            catch (OperationCanceledException ex)
            {
                exception = ex;
                deliveryResponse.DeliveryOutcome = RequestOutcome.CANCELLED;
                deliveryResponse.DeliveryDetail = ex.Message;
            }
            catch (Exception ex)
            {
                exception = ex;
                deliveryResponse.DeliveryOutcome = RequestOutcome.FAILED_UNKNOWN;
                deliveryResponse.DeliveryDetail = ex.Message;
            }

            if (deliveryResponse.DeliveryOutcome == RequestOutcome.SUCCESS)
            {
                deliveryRequest.Succeed();
                MessageDispatcherEventSource.Current.Info(taskInfo.OutputMessage.MessageInfo.TrackingId, this, nameof(this.DeliverAsync), OperationStates.Succeeded, $"msgId={taskInfo.OutputMessage.MessageInfo.MessageId}, batchId={taskInfo.OutputMessage.Id}, E2ELantency={DateTime.UtcNow - taskInfo.OutputMessage.MessageInfo.SendTime}, PushLatency={DateTime.UtcNow - deliveryRequest.DeliverRequestTimestamp}, delivered={taskInfo.OutputMessage.Delivered}, requestExpiration={taskInfo.OutputMessage.RequestExpiration}, deliveryResponse={deliveryResponse}");
            }
            else
            {
                if (deliveryRequest != null)
                {
                    deliveryRequest.Failed();
                    if (deliveryResponse.DeliveryOutcome == RequestOutcome.FAILED_UNAUTHORIZED)
                    {
                        // Need to refilter if authentication failed
                        taskInfo.OutputMessage.State = OutputMessageState.Nonfiltered;
                    }
                    else
                    {
                        taskInfo.OutputMessage.State = OutputMessageState.FilteredFailingDelivery;
                    }

                    MessageDispatcherEventSource.Current.Error(taskInfo.OutputMessage.MessageInfo.TrackingId, this, nameof(this.DeliverAsync), OperationStates.Failed, $"msgId={taskInfo.OutputMessage.MessageInfo.MessageId}, batchId={taskInfo.OutputMessage.Id}, outcome={deliveryResponse.DeliveryOutcome}, latency={DateTime.UtcNow - deliveryRequest.DeliverRequestTimestamp}, delivered={taskInfo.OutputMessage.Delivered}, requestExpiration={taskInfo.OutputMessage.RequestExpiration}, deliveryResponse={deliveryResponse}, exception={exception}");
                }
                else
                {
                    MessageDispatcherEventSource.Current.Error(taskInfo.OutputMessage.MessageInfo.TrackingId, this, nameof(this.DeliverAsync), OperationStates.Failed, $"msgId={taskInfo.OutputMessage.MessageInfo.MessageId}, batchId={taskInfo.OutputMessage.Id}, outcome={deliveryResponse.DeliveryOutcome}, timestamp={DateTime.UtcNow}, delivered={taskInfo.OutputMessage.Delivered}, requestExpiration={taskInfo.OutputMessage.RequestExpiration}, deliveryResponse={deliveryResponse}, exception={exception}");
                }
            }

            return deliveryResponse;
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
