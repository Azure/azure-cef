// <copyright file="IMessageDispatcher.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public interface IMessageDispatcher : IComponent
    {
        Task<Task> DispatchAsync(IList<OutputMessage> outputMessages);

        Task PostponedAsync(List<PushTaskInfo> taskInfos);
    }
}
