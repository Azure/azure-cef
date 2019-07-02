// <copyright file="ReportAgent.cs" company="Microsoft Corporation">
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

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    public class ReportAgent
    {
        private static readonly Random Random = new Random();

        // AgentId should be in format of EmailAgent_{ConnnecterName}_{ConnectorId}_{EngagementAccount}_{Random}
        // This is to throttle the max concurrency of operations for a single account
        private static readonly string AgentIdFormat = "EmailEngineAgent_{0}_{1}_{2}_{3}";

        private ConnectorCredential credential;
        private EmailAccount emailAccount;
        private ServiceConfiguration configuration;
        private Uri serviceUri;
        private ActorId actorId;
        private IEmailConnector connector;

        public ReportAgent(ConnectorCredential credential, EmailAccount emailAccount, ServiceConfiguration configuration)
        {
            this.credential = credential;
            this.emailAccount = emailAccount;
            this.configuration = configuration;
            this.serviceUri = new Uri(credential.ConnectorUri);
            this.actorId = new ActorId(string.Format(AgentIdFormat, credential.ConnectorName, credential.ConnectorId, emailAccount.EngagementAccount, Random.Next(1, this.configuration.ActorReportMaxCount)));
            this.connector = ActorProxy.Create<IEmailConnector>(this.actorId, this.serviceUri);
        }

        public async Task<ReportList> GetReportsAsync(List<MessageIdentifer> messageIdentifers, CancellationToken cancellationToken)
        {
            return await this.connector.GetMailReportAsync(this.credential, this.emailAccount, messageIdentifers, cancellationToken);
        }
    }
}
