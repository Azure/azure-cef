// <copyright file="CredentialManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.EmailProvider.Store;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Credential
{
    public class CredentialManager : ICredentialManager
    {
        private IEmailStore store;
        private ConcurrentDictionary<string, ConnectorMetadata> connectorMetadataCache;

        public CredentialManager(IEmailStoreFactory factory)
        {
            this.store = factory.GetStore();
            this.connectorMetadataCache = new ConcurrentDictionary<string, ConnectorMetadata>();
        }

        #region Connector Metadata

        public async Task<ConnectorMetadata> GetMetadataAsync(string connectorName)
        {
            // Try to get from cache first
            if (this.connectorMetadataCache.TryGetValue(connectorName, out ConnectorMetadata metadata))
            {
                return metadata;
            }

            // Get from db and update in cache
            metadata = await this.store.GetConnectorMetadataAsync(connectorName);
            Validator.IsTrue<ResourceNotFoundException>(metadata != null, nameof(metadata), "Credential Metadata '{0}' does not exist.", connectorName);

            this.connectorMetadataCache.TryAdd(connectorName, metadata);

            return metadata;
        }

        #endregion

        #region Connector Credential

        public async Task CreateOrUpdateConnectorCredentialAsync(EmailConnectorCredential credential)
        {
            Validator.ArgumentNotNull(credential, nameof(credential));
            Validator.ArgumentNotNullOrEmpty(credential.ConnectorName, nameof(credential.ConnectorName));
            Validator.ArgumentNotNullOrEmpty(credential.ConnectorId, nameof(credential.ConnectorId));

            await this.store.CreateOrUpdateCredentialAsync(credential);
        }

        public async Task<EmailConnectorCredential> GetConnectorCredentialByIdAsync(ConnectorIdentifier identifier)
        {
            var credential = await this.store.GetConnectorCredentialByIdAsync(identifier);
            Validator.IsTrue<ResourceNotFoundException>(credential != null, nameof(credential), "Credential '{0}' does not exist or is disabled.", identifier);

            return credential;
        }

        public async Task DeleteConnectorCredentialAsync(ConnectorIdentifier identifier)
        {
            var credential = GetConnectorCredentialByIdAsync(identifier);
            await this.store.DeleteConnectorCredentialAsync(identifier);
        }

        #endregion

        #region Connector Credential Assignment

        public async Task CreateOrUpdateCredentialAssignmentAsync(CredentialAssignment credentialAssignment)
        {
            var credential = this.GetConnectorCredentialByIdAsync(credentialAssignment.ConnectorIdentifier);
            await this.store.CreateOrUpdateCredentialAssignmentAsync(credentialAssignment);
        }

        public async Task<CredentialAssignment> GetCredentialAssignmentByAccountAsync(string engagementAccount)
        {
            var assignments = await this.store.ListCredentialAssignmentsByAccountAsync(engagementAccount, true);
            Validator.IsTrue<ResourceNotFoundException>(assignments != null && assignments.Count > 0, nameof(assignments), "No active credetial assignment for account '{0}'.", engagementAccount);

            return assignments.FirstOrDefault();
        }

        public async Task DeleteCredentialAssignmentsAsync(string engagementAccount, ConnectorIdentifier identifier)
        {
            var assignments = await this.store.ListCredentialAssignmentsByAccountAsync(engagementAccount, false);
            Validator.IsTrue<ResourceNotFoundException>(assignments != null || assignments.Any(a => a.ConnectorIdentifier.Equals(identifier)), nameof(identifier), "The assignment does not exist.");

            await this.store.DeleteCredentialAssignmentsAsync(engagementAccount, identifier);
        }

        public async Task<List<CredentialAssignment>> ListCredentialAssignmentsByAccountAsync(string engagementAccount, bool activeOnly)
        {
            return await this.store.ListCredentialAssignmentsByAccountAsync(engagementAccount, activeOnly);
        }

        public async Task<List<CredentialAssignment>> ListCredentialAssignmentsById(ConnectorIdentifier identifier, bool activeOnly = true)
        {
            return await this.store.ListCredentialAssignmentsById(identifier, activeOnly);
        }

        #endregion

        #region Email Account & Contract

        public async Task<ConnectorCredential> GetConnectorCredentialContractAsync(string account)
        {
            // Get Credential Assignment
            var assignment = await this.GetCredentialAssignmentByAccountAsync(account);

            // Get Connector Metadata
            var metadata = await this.GetMetadataAsync(assignment.ConnectorIdentifier.ConnectorName);

            // Get Connector Credential
            var credential = await this.GetConnectorCredentialByIdAsync(assignment.ConnectorIdentifier);

            // Convert to Contract
            return credential.ToDataContract(metadata);
        }

        public async Task<EmailAccount> EnsureEmailAccountAsync(string account)
        {
            // Get EmailAccount
            var emailAccount = await this.GetEmailAccountAsync(account);
            Validator.IsTrue<ApplicationException>(emailAccount.Domains != null && emailAccount.Domains.Count > 0, nameof(emailAccount.Domains), "No active domains for account {0}", account);

            // Get Connector Credential Contract
            var credential = await this.GetConnectorCredentialContractAsync(account);

            // Request to connector
            var connector = new CredentialAgent(credential, account);
            emailAccount = await connector.CreateEmailAccountAsync(credential, emailAccount, CancellationToken.None);

            // Update EmailAccount
            emailAccount = await this.store.UpdateEmailAccountAsync(emailAccount);
            return emailAccount;
        }

        public async Task CleanupEmailAccountAsync(string account)
        {
            // Get EmailAccount
            var emailAccount = await this.GetEmailAccountAsync(account);

            // Get Connector Credential Contract
            var credential = await this.GetConnectorCredentialContractAsync(account);

            // Request to connector
            var connector = new CredentialAgent(credential, account);
            await connector.DeleteEmailAccountAsync(credential, emailAccount, CancellationToken.None);
        }

        public async Task<EmailAccount> GetEmailAccountAsync(string account)
        {
            // Get EmailAccount
            var emailAccount = await this.store.GetEmailAccountAsync(account);
            Validator.IsTrue<ResourceNotFoundException>(emailAccount != null, nameof(emailAccount), "Account {0} does not exist", account);

            return emailAccount;
        }

        #endregion
    }
}
