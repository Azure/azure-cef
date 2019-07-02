// <copyright file="InboundAgent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Inbound
{
    public class InboundAgent : IDisposable
    {
        private static readonly string AgentIdFormat = "InboundAgent_{0}";

        private Uri serviceUri;
        private ActorId actorId;
        private ISmsConnector connector;

        public InboundAgent(string connectorUri)
        {
            this.serviceUri = new Uri(connectorUri);
            this.actorId = new ActorId(string.Format(AgentIdFormat, Guid.NewGuid().ToString()));
            this.connector = ActorProxy.Create<ISmsConnector>(this.actorId, this.serviceUri);
        }

        public Task<string> ParseConnectorIdFromInboundMessageAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestContract = new InboundHttpRequestMessage(request);
            return this.connector.ParseConnectorIdFromInboundMessageAsync(requestContract, cancellationToken);
        }

        public Task<InboundResponse> ParseInboundRequestAsync(HttpRequestMessage request, ConnectorCredential credential, CancellationToken cancellationToken)
        {
            var requestContract = new InboundHttpRequestMessage(request);
            return this.connector.ParseInboundRequestAsync(requestContract, credential, cancellationToken);
        }

        public Task<List<string>> ParseExtendedCodeAsync(string extendedCode, List<int> segmentLengths, CancellationToken cancellationToken)
        {
            return this.connector.ParseExtendedCodeAsync(extendedCode, segmentLengths, cancellationToken);
        }

        public void Dispose()
        {
            var serviceProxy = ActorServiceProxy.Create(this.serviceUri, this.actorId);
            TaskHelper.FireAndForget(() => serviceProxy.DeleteActorAsync(actorId, CancellationToken.None));
        }
    }
}
