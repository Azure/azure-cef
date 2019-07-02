// <copyright file="IDispatcherConnector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.ServiceFabric.Actors;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface
{
    public interface IDispatcherConnector : IActor
    {
        Task<DeliveryResponse> DeliverAsync(DeliveryRequest deliveryRequest, CancellationToken cancellationToken);
    }
}
