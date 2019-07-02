// <copyright file="IPushWorker.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public interface IPushWorker : IComponent
    {
        int QueueLength { get; }

        bool TryAdd(List<PushTaskInfo> tasks);
    }
}
