// <copyright file="IMessageProcessor.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public interface IMessageProcessor<TMessage> : IComponent
    {
        Task AppendAsync(IReadOnlyList<TMessage> events);
    }
}
