// <copyright file="AgentMetadata.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    public class AgentMetadata : ConnectorIdentifier
    {
        public DateTime? LastMessageSendTime { get; set; }

        public DateTime? LastReportUpdateTime { get; set; }

        public long PendingReceive { get; set; }

        public ConnectorIdentifier GetIdentifier()
        {
            return new ConnectorIdentifier(this.ConnectorName, this.ConnectorId);
        }
    }
}
