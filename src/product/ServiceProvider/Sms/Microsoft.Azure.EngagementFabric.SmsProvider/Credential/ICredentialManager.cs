// <copyright file="ICredentialManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Credential
{
    public interface ICredentialManager
    {
        // Connector Metadata
        Task<ConnectorMetadata> GetMetadata(string connectorName);

        // Connector Credential
        Task CreateOrUpdateConnectorCredentialAsync(SmsConnectorCredential credential);

        Task<SmsConnectorCredential> GetConnectorCredentialByIdAsync(ConnectorIdentifier identifier);

        Task DeleteConnectorCredentialAsync(ConnectorIdentifier identifier);

        // Connector Credential Assignment
        Task CreateOrUpdateCredentialAssignmentAsync(ConnectorCredentialAssignment credentialAssignment);

        Task<ConnectorCredentialAssignment> GetCredentialAssignmentByAccountAsync(string engagementAccount, ChannelType channelType);

        Task DeleteCredentialAssignmentsAsync(string engagementAccount, ConnectorIdentifier identifier);

        Task<List<ConnectorCredentialAssignment>> ListCredentialAssignmentsByAccountAsync(string engagementAccount, ChannelType channelType, bool activeOnly);

        Task<List<ConnectorCredentialAssignment>> ListCredentialAssignmentsById(ConnectorIdentifier identifier, bool activeOnly = true);
    }
}
