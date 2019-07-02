// <copyright file="PushTaskInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract
{
    public class PushTaskInfo
    {
        public OutputMessage OutputMessage { get; set; }

        public TaskCompletionSource<bool> Tcs { get; set; }

        public DateTime PushEnqueueTimestamp { get; set; }
    }
}
