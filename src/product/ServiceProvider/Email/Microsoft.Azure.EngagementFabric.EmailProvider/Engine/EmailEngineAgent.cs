// <copyright file="EmailEngineAgent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Configuration;
using Microsoft.Azure.EngagementFabric.EmailProvider.Utils;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Engine
{
    public class EmailEngineAgent
    {
        private static readonly Random Random = new Random();

        // AgentId should be in format of EmailAgent_{ConnnecterName}_{ConnectorId}_{EngagementAccount}_{Random}
        // This is to throttle the max concurrency of operations for a single account
        private static readonly string AgentIdFormat = "EmailEngineAgent_{0}_{1}_{2}_{3}";

        private Uri serviceUri;
        private ActorId actorId;
        private IEmailConnector connector;

        public EmailEngineAgent(ConnectorCredential credential, string engagementAccount, ServiceConfiguration configuration)
        {
            this.serviceUri = new Uri(credential.ConnectorUri);
            this.actorId = new ActorId(string.Format(AgentIdFormat, credential.ConnectorName, credential.ConnectorId, engagementAccount, Random.Next(1, configuration.ActorAccountMaxCount)));
            this.connector = ActorProxy.Create<IEmailConnector>(this.actorId, this.serviceUri);
        }

        // SenderAddress
        public Task<SenderAddress> CreateorUpdateSenderAddressAsync(ConnectorCredential credential, EmailAccount emailAccount, SenderAddress senderAddress, CancellationToken cancellationToken)
        {
            return this.connector.CreateorUpdateSenderAddressAsync(credential, emailAccount, senderAddress, cancellationToken);
        }

        public Task DeleteSenderAddressAsync(ConnectorCredential credential, EmailAccount emailAccount, List<SenderAddress> senderAddressList, CancellationToken cancellationToken)
        {
            return this.connector.DeleteSenderAddressAsync(credential, emailAccount, senderAddressList, cancellationToken);
        }

        // Group
        public Task<GroupCreateOrUpdateResult> CreateorUpdateGroupAsync(ConnectorCredential credential, EmailAccount emailAccount, Group group, CancellationToken cancellationToken)
        {
            return this.connector.CreateorUpdateGroupAsync(credential, emailAccount, group, cancellationToken);
        }

        public Task<GroupMembers> GetGroupMembersAsync(ConnectorCredential credential, EmailAccount emailAccount, Group group, GroupMemberRequest request, CancellationToken cancellationToken)
        {
            return this.connector.GetGroupMembersAsync(credential, emailAccount, group, request, cancellationToken);
        }

        public Task DeleteGroupAsync(ConnectorCredential credential, EmailAccount emailAccount, List<Group> groupList, CancellationToken cancellationToken)
        {
            return this.connector.DeleteGroupAsync(credential, emailAccount, groupList, cancellationToken);
        }

        // Mailing
        public Task DeleteMailingAsync(ConnectorCredential credential, EmailAccount emailAccount, List<MessageIdentifer> messageIdentifers, CancellationToken cancellationToken)
        {
            return this.connector.DeleteMailingAsync(credential, emailAccount, messageIdentifers, cancellationToken);
        }
    }
}
