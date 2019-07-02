// <copyright file="AsyncSignal.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.Common.Threading
{
    public class AsyncSignal : IDisposable
    {
        private readonly TimeSpan timeout;
        private SpinLock spinLock;
        private TaskCompletionSource<bool> tcs;
        private bool pendingSignal;
        private bool disposed;

        public AsyncSignal(TimeSpan timeout)
        {
            this.timeout = timeout;
            this.spinLock = new SpinLock(enableThreadOwnerTracking: false);
            this.tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            this.tcs.SetResult(true);
        }

        public Task WaitAsync()
        {
            TaskCompletionSource<bool> newTcs;
            bool lockTaken = false;
            try
            {
                this.spinLock.Enter(ref lockTaken);

                if (this.pendingSignal || this.timeout <= TimeSpan.Zero || this.disposed)
                {
                    this.pendingSignal = false;
                    return Task.CompletedTask;
                }

                if (!this.tcs.Task.IsCompleted)
                {
                    throw new InvalidOperationException($"{nameof(WaitAsync)} is already been invoked. It can only be invoked once at a time.");
                }

                newTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                this.tcs = newTcs;
            }
            finally
            {
                if (lockTaken)
                {
                    this.spinLock.Exit();
                }
            }

            if (this.timeout < TimeSpan.MaxValue)
            {
                Timer timeoutTimer = new Timer((s) => newTcs.TrySetResult(true), null, (int)this.timeout.TotalMilliseconds, Timeout.Infinite);
                newTcs.Task.ContinueWith((t) => timeoutTimer.Dispose());
            }

            return newTcs.Task;
        }

        public void Set()
        {
            bool lockTaken = false;
            try
            {
                this.spinLock.Enter(ref lockTaken);
                if (this.tcs.Task.IsCompleted || !this.tcs.TrySetResult(true))
                {
                    this.pendingSignal = true;
                }
            }
            finally
            {
                if (lockTaken)
                {
                    this.spinLock.Exit();
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    bool lockTaken = false;
                    try
                    {
                        this.spinLock.Enter(ref lockTaken);
                        this.disposed = true;
                        this.tcs.TrySetResult(true);
                    }
                    finally
                    {
                        if (lockTaken)
                        {
                            this.spinLock.Exit();
                        }
                    }
                }

                this.disposed = true;
            }
        }
    }
}
