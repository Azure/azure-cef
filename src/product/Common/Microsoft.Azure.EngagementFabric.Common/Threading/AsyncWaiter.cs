// <copyright file="AsyncWaiter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.Common.Threading
{
    public sealed class AsyncWaiter
    {
        private readonly object syncLock;
        private CancellationTokenSource waitCancellation;
        private bool signaled;
        private int waiterCount;
        private bool isClosed;

        public AsyncWaiter()
        {
            this.waitCancellation = new CancellationTokenSource();
            this.syncLock = new object();
        }

        public async Task<bool> WaitAsync(TimeSpan timeout, CancellationToken cancelToken)
        {
            CancellationTokenSource linkedTokenSource = null;
            try
            {
                Task<bool> delayTask;
                lock (this.syncLock)
                {
                    if (this.signaled)
                    {
                        return true;
                    }
                    else if (this.isClosed)
                    {
                        return false;
                    }

                    linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.waitCancellation.Token, cancelToken);
                    delayTask = TaskHelper.TryDelay(timeout, linkedTokenSource.Token);
                }

                // delayTaskResult is true if timeout elapsed, false if the cancellationToken was cancelled.
                bool delayTaskResult = await delayTask;
                if (this.isClosed)
                {
                    return false;
                }

                return !delayTaskResult;
            }
            finally
            {
                linkedTokenSource?.Dispose();
            }
        }

        public void Close()
        {
            this.isClosed = true;
            this.Reset();
        }

        public void Reset()
        {
            lock (this.syncLock)
            {
                this.waiterCount = 0;
                this.signaled = false;
                if (this.waitCancellation != null)
                {
                    if (!this.waitCancellation.IsCancellationRequested)
                    {
                        this.waitCancellation.Cancel();
                        this.waitCancellation.Dispose();
                        this.waitCancellation = null;
                    }
                }
            }
        }

        public void Initialize()
        {
            lock (this.syncLock)
            {
                this.waitCancellation = new CancellationTokenSource();
                this.waiterCount++;
            }
        }

        public void Set()
        {
            lock (this.syncLock)
            {
                if (this.waiterCount > 0)
                {
                    this.signaled = true;
                    if (!this.waitCancellation.IsCancellationRequested)
                    {
                        this.waitCancellation.Cancel(false);
                    }
                }
            }
        }
    }
}
