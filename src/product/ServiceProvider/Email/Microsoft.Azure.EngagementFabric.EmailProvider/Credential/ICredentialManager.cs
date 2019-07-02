// <copyright file="ICredentialManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Credential
{
    public interface ICredentialManager
    {
        // Connector Metadata
        Task<ConnectorMetadata> GetMetadataAsync(string connectorName);

        // Connector Credential
        Task CreateOrUpdateConnectorCredentialAsync(EmailConnectorCredential credential);

        Task<EmailConnectorCredential> GetConnectorCredentialByIdAsync(ConnectorIdentifier identifier);

        Task DeleteConnectorCredentialAsync(ConnectorIdentifier identifier);

        // Connector Credential Assignment
        Task CreateOrUpdateCredentialAssignmentAsync(CredentialAssignment credentialAssignment);

        Task<CredentialAssignment> GetCredentialAssignmentByAccountAsync(string engagementAccount);

        Task DeleteCredentialAssignmentsAsync(string engagementAccount, ConnectorIdentifier identifier);

        Task<List<CredentialAssignment>> ListCredentialAssignmentsByAccountAsync(string engagementAccount, bool activeOnly);

        Task<List<CredentialAssignment>> ListCredentialAssignmentsById(ConnectorIdentifier identifier, bool activeOnly = true);

        // Email Account
        Task<ConnectorCredential> GetConnectorCredentialContractAsync(string account);

        Task<EmailAccount> EnsureEmailAccountAsync(string account);

        Task CleanupEmailAccountAsync(string account);

        Task<EmailAccount> GetEmailAccountAsync(string account);
    }
}
