// <copyright file="IMessageFilteringEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.FilteringEngine
{
    public interface IMessageFilteringEngine<TMessage> : IComponent
    {
        Task<IList<OutputMessage>> FilterAsync(IReadOnlyList<TMessage> messages, CancellationToken cancellationToken);
    }
}
