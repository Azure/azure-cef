// <copyright file="IDispatcherService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface
{
    public interface IDispatcherService : IService
    {
        Task DispatchAsync(List<InputMessage> messages, CancellationToken cancellationToken);
    }
}
