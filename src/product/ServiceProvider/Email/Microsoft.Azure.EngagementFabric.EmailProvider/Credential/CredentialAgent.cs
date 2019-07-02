// <copyright file="CredentialAgent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Credential
{
    public class CredentialAgent
    {
        // AgentId should be in format of EmailAgent_{ConnnecterName}_{ConnectorId}_{EngagementAccount}
        // This is to enable parallel account operations under the same connector, but disable parallel operation for the same account
        private static readonly string AgentIdFormat = "EmailAgent_{0}_{1}_{2}";

        private Uri serviceUri;
        private ActorId actorId;
        private IEmailConnector connector;

        public CredentialAgent(ConnectorCredential credential, string engagementAccount)
        {
            this.serviceUri = new Uri(credential.ConnectorUri);
            this.actorId = new ActorId(string.Format(AgentIdFormat, credential.ConnectorName, credential.ConnectorId, engagementAccount));
            this.connector = ActorProxy.Create<IEmailConnector>(this.actorId, this.serviceUri);
        }

        // Account
        public Task<EmailAccount> CreateEmailAccountAsync(ConnectorCredential credential, EmailAccount emailAccount, CancellationToken cancellationToken)
        {
            return this.connector.CreateEmailAccountAsync(credential, emailAccount, cancellationToken);
        }

        public Task DeleteEmailAccountAsync(ConnectorCredential credential, EmailAccount emailAccount, CancellationToken cancellationToken)
        {
            return this.connector.DeleteEmailAccountAsync(credential, emailAccount, cancellationToken);
        }
    }
}
