// <copyright file="TaskHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.Common.Threading
{
    public static class TaskHelper
    {
        // Avoid to fire TaskCanceledException
        public static async Task<bool> TryDelay(TimeSpan delay, CancellationToken cancellationToken)
        {
            try
            {
                await Task.Delay(delay, cancellationToken);
                return true;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
        }

        public static void FireAndForget(Func<Task> task)
        {
            Task.Run(() => Fire(task, (ex) => { }));
        }

        public static void FireAndForget(Func<Task> task, Action<Exception> onUnhandledException)
        {
            Task.Run(() => Fire(task, onUnhandledException));
        }

        private static async Task Fire(Func<Task> task, Action<Exception> onUnhandledException)
        {
            try
            {
                await task();
            }
            catch (Exception ex)
            {
                onUnhandledException?.Invoke(ex);
            }
        }
    }
}
