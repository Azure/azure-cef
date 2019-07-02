// <copyright file="ConnectorCredentialAssignment.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Credential
{
    public class ConnectorCredentialAssignment
    {
        public string EngagementAccount { get; set; }

        public ChannelType ChannelType { get; set; }

        public ConnectorIdentifier ConnectorIdentifier { get; set; }

        public bool Enabled { get; set; }

        public bool Active { get; set; }

        public string ExtendedCode { get; set; }
    }
}
