// <copyright file="ISmsStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Report;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Store
{
    public interface ISmsStore
    {
        // Connector Metadata
        Task<ConnectorMetadata> GetConnectorMetadataAsync(string connectorName);

        // Connector Credential
        Task CreateOrUpdateCredentialAsync(SmsConnectorCredential credential);

        Task<SmsConnectorCredential> GetConnectorCredentialByIdAsync(ConnectorIdentifier identifier);

        Task DeleteConnectorCredentialAsync(ConnectorIdentifier identifier);

        // Connector Credential Assignment
        Task CreateOrUpdateCredentialAssignmentAsync(ConnectorCredentialAssignment credentialAssignment);

        Task DeleteCredentialAssignmentsAsync(string engagementAccount, ConnectorIdentifier identifier);

        Task<List<ConnectorCredentialAssignment>> ListCredentialAssignmentsByAccountAsync(string engagementAccount, ChannelType channelType, bool activeOnly);

        Task<List<ConnectorCredentialAssignment>> ListCredentialAssignmentsById(ConnectorIdentifier identifier, bool activeOnly = true);

        // Account
        Task<Account> CreateOrUpdateAccountAsync(Account account);

        Task<Account> GetAccountAsync(string engagementAccount);

        Task DeleteAccountAsync(string engagementAccount);

        // Signature
        Task<Signature> CreateOrUpdateSignatureAsync(Signature signature);

        Task<Signature> GetSignatureAsync(string engagementAccount, string signature);

        Task<SignatureList> ListSignaturesAsync(string engagementAccount, DbContinuationToken continuationToken, int count);

        Task DeleteSignatureAsync(string engagementAccount, string signature);

        Task DeleteSignaturesAsync(string engagementAccount);

        // Template
        Task<Template> CreateOrUpdateTemplateAsync(Template template);

        Task<Template> GetTemplateAsync(string engagementAccount, string template);

        Task<TemplateList> ListTemplatesAsync(string engagementAccount, DbContinuationToken continuationToken, int count);

        Task DeleteTemplateAsync(string engagementAccount, string template);

        Task DeleteTemplateBySignatureAsync(string engagementAccount, string signature);

        Task DeleteTemplatesAsync(string engagementAccount);

        Task UpdateTemplateStateBySignatureAsync(string engagementAccount, string signature, ResourceState fromState, ResourceState toState, string message = null);

        // Connector agent metadata (for pulling report)
        Task CreateOrUpdateAgentMetadataAsync(AgentMetadata metadata);

        Task<AgentMetadata> GetAgentMetadataAsync(ConnectorIdentifier identifier);

        Task<List<AgentMetadata>> ListAgentMetadataAsync();
    }
}
