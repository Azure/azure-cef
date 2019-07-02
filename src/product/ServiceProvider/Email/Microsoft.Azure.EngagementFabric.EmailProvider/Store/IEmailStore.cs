// <copyright file="IEmailStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Credential;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Group = Microsoft.Azure.EngagementFabric.EmailProvider.Model.Group;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Store
{
    public interface IEmailStore
    {
        // Connector Metadata
        Task<ConnectorMetadata> GetConnectorMetadataAsync(string connectorName);

        // Connector Credential
        Task CreateOrUpdateCredentialAsync(EmailConnectorCredential credential);

        Task<EmailConnectorCredential> GetConnectorCredentialByIdAsync(ConnectorIdentifier identifier);

        Task DeleteConnectorCredentialAsync(ConnectorIdentifier identifier);

        // Connector Credential Assignment
        Task CreateOrUpdateCredentialAssignmentAsync(CredentialAssignment credentialAssignment);

        Task DeleteCredentialAssignmentsAsync(string engagementAccount, ConnectorIdentifier identifier);

        Task<List<CredentialAssignment>> ListCredentialAssignmentsByAccountAsync(string engagementAccount, bool activeOnly);

        Task<List<CredentialAssignment>> ListCredentialAssignmentsById(ConnectorIdentifier identifier, bool activeOnly = true);

        // Account
        Task<Account> CreateOrUpdateAccountAsync(Account account);

        Task<Account> GetAccountAsync(string engagementAccount);

        Task DeleteAccountAsync(string engagementAccount);

        // Domain
        Task<Domain> CreateOrUpdateDomainAsync(Domain domain);

        Task<List<Domain>> GetDomainsByNameAsync(string domain);

        Task<Domain> GetDomainAsync(string engagementAccount, string domain);

        Task<DomainList> ListDomainsByAccountAsync(string engagementAccount, DbContinuationToken continuationToken, int count);

        Task DeleteDomainAsync(string engagementAccount, string domain);

        Task DeleteDomainsAsync(string engagementAccount);

        // Group
        Task<Group> CreateOrUpdateGroupAsync(Group group);

        Task<Group> GetGroupAsync(string engagementAccount, string group);

        Task<GroupList> ListGroupsAsync(string engagementAccount, DbContinuationToken continuationToken, int count);

        Task DeleteGroupAsync(string engagementAccount, string group);

        Task DeleteGroupsAsync(string engagementAccount);

        // Sender
        Task<Sender> CreateOrUpdateSenderAsync(Sender sender);

        Task<Sender> GetSenderByIdAsync(string engagementAccount, Guid senderId);

        Task<Sender> GetSenderByNameAsync(string engagementAccount, string senderAddr);

        Task<SenderList> ListSendersAsync(string engagementAccount, DbContinuationToken continuationToken, int count);

        Task DeleteSenderAsync(string engagementAccount, Guid senderId);

        Task<List<Sender>> GetSendersByDomainAsync(string engagementAccount, string domain);

        Task DeleteSendersByDomainAsync(string engagementAccount, string domain);

        Task DeleteSendersAsync(string engagementAccount);

        // Template
        Task<Template> CreateOrUpdateTemplateAsync(Template template, Sender sender);

        Task<Template> GetTemplateAsync(string engagementAccount, string template);

        Task<TemplateList> ListTemplatesAsync(string engagementAccount, DbContinuationToken continuationToken, int count);

        Task DeleteTemplateAsync(string engagementAccount, string template);

        Task DeleteTemplatesAsync(string engagementAccount);

        Task UpdateTemplateStateByDomainAsync(string engagementAccount, string domain, ResourceState fromState, ResourceState toState, string message = null);

        Task DeleteTemplatesByDomainAsync(string engagementAccount, string domain);

        Task DeleteTemplatesBySenderAsync(string engagementAccount, Guid senderId);

        // EmailAccount
        Task<EmailAccount> GetEmailAccountAsync(string engagementAccount);

        Task<EmailAccount> UpdateEmailAccountAsync(EmailAccount emailAccount);
    }
}
