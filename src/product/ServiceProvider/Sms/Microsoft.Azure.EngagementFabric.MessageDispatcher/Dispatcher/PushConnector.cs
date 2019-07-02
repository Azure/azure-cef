// <copyright file="PushConnector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.DispatcherInterface;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public class PushConnector : ITraceStateProvider
    {
        private ConnectorCredential connectorInfo;

        public PushConnector(ConnectorCredential connectorInfo)
        {
            this.connectorInfo = connectorInfo;
        }

        public static PushConnector Create(ConnectorCredential connectorInfo)
        {
            return new PushConnector(connectorInfo);
        }

        public string GetTraceState()
        {
            return $"name={this.connectorInfo.ConnectorName} id={this.connectorInfo.ConnectorId}";
        }

        public async Task<DeliveryResponse> DeliverAsync(DeliveryRequest deliveryRequest, CancellationToken cancellationToken)
        {
            try
            {
                var serviceUri = new Uri(connectorInfo.ConnectorUri);
                var actorId = new ActorId(deliveryRequest.OutputMessage.Id);

                // Create actor instance
                var connector = ActorProxy.Create<IDispatcherConnector>(actorId, serviceUri);

                // Dispatch
                var response = await connector.DeliverAsync(deliveryRequest, cancellationToken);

                // Release the actor resource at once
                var serviceProxy = ActorServiceProxy.Create(serviceUri, actorId);
                TaskHelper.FireAndForget(() => serviceProxy.DeleteActorAsync(actorId, cancellationToken));

                return response;
            }
            catch (Exception ex)
            {
                MessageDispatcherEventSource.Current.ErrorException(deliveryRequest.OutputMessage.MessageInfo.TrackingId, this, nameof(this.DeliverAsync), OperationStates.Failed, string.Empty, ex);
                return new DeliveryResponse(RequestOutcome.UNKNOWN);
            }
        }
    }
}
