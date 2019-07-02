// <copyright file="RunAsyncComponent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Common
{
    public abstract class RunAsyncComponent : CancelTokenComponent
    {
        private Task runTask;

        protected RunAsyncComponent(string component)
            : base(component)
        {
        }

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.runTask = Task.Run(() => this.RunAsync());
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            // Wait for any inflight work to complete and the task to end gracefully.
            if (this.runTask != null)
            {
                var tcs = new TaskCompletionSource<bool>();
                using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
                {
                    if (this.runTask != await Task.WhenAny(this.runTask, tcs.Task))
                    {
                        throw new OperationCanceledException(cancellationToken);
                    }
                }
            }
        }

        protected virtual async Task OnRunAsync(CancellationToken cancelToken)
        {
            var startTaskFuncs = this.OnRunMultiple();
            if (startTaskFuncs?.Length > 0)
            {
                // Start all the tasks
                var runTasks = new List<Task>(startTaskFuncs.Length);
                foreach (Func<CancellationToken, Task> startTaskFunc in startTaskFuncs)
                {
                    Task runTask = Task.Run(() => startTaskFunc(this.CancelToken));
                    runTasks.Add(runTask);
                }

                // Wait for the tasks to complete/fault.
                while (runTasks.Count > 0)
                {
                    // Wait for any task to complete (possibly with Exception), that way we'll fault
                    // ourselves when it happens as opposed to only at shutdown time.
                    Task completedTask = await Task.WhenAny(runTasks);
                    if (completedTask.IsFaulted)
                    {
                        // Rethrow the inner exception to get rid of AggregateException wrapper.  Use Rethrow to keep the original stack.
                        throw completedTask.Exception.InnerException;
                    }

                    runTasks.Remove(completedTask);
                }
            }
        }

        protected virtual Func<CancellationToken, Task>[] OnRunMultiple()
        {
            return new Func<CancellationToken, Task>[0];
        }

        private async Task RunAsync()
        {
            try
            {
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(RunAsync), OperationStates.Starting, string.Empty);
                await this.OnRunAsync(this.CancelToken);
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(RunAsync), OperationStates.Succeeded, string.Empty);
            }
            catch (Exception e)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(RunAsync), OperationStates.Faulting, string.Empty, e);
                this.Fault(e);
            }
        }
    }
}
