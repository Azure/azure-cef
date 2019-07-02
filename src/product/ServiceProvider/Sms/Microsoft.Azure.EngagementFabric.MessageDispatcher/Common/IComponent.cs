// <copyright file="IComponent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Common
{
    public interface IComponent
    {
        event EventHandler Opened;

        event EventHandler Closed;

        event EventHandler<FirstChanceExceptionEventArgs> Faulted;

        Task OpenAsync(CancellationToken cancellationToken);

        Task CloseAsync(CancellationToken cancellationToken);
    }
}
