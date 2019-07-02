// <copyright file="CredentialManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Store;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Credential
{
    public class CredentialManager : ICredentialManager
    {
        private ISmsStore store;
        private ConcurrentDictionary<string, ConnectorMetadata> connectorMetadataCache;

        public CredentialManager(ISmsStoreFactory factory)
        {
            this.store = factory.GetStore();
            this.connectorMetadataCache = new ConcurrentDictionary<string, ConnectorMetadata>();
        }

        #region Connector Metadata

        public async Task<ConnectorMetadata> GetMetadata(string connectorName)
        {
            // Try to get from cache first
            if (this.connectorMetadataCache.TryGetValue(connectorName, out ConnectorMetadata metadata))
            {
                return metadata;
            }

            // Get from db and update in cache
            metadata = await this.store.GetConnectorMetadataAsync(connectorName);
            this.connectorMetadataCache.TryAdd(connectorName, metadata);

            return metadata;
        }

        #endregion

        #region Connector Credential

        public async Task CreateOrUpdateConnectorCredentialAsync(SmsConnectorCredential credential)
        {
            Validator.ArgumentNotNull(credential, nameof(credential));
            Validator.ArgumentNotNullOrEmpty(credential.ConnectorName, nameof(credential.ConnectorName));
            Validator.ArgumentNotNullOrEmpty(credential.ConnectorId, nameof(credential.ConnectorId));

            await this.store.CreateOrUpdateCredentialAsync(credential);
        }

        public async Task<SmsConnectorCredential> GetConnectorCredentialByIdAsync(ConnectorIdentifier identifier)
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

        public async Task CreateOrUpdateCredentialAssignmentAsync(ConnectorCredentialAssignment credentialAssignment)
        {
            var credential = await this.store.GetConnectorCredentialByIdAsync(credentialAssignment.ConnectorIdentifier);
            Validator.IsTrue<ResourceNotFoundException>(credential != null, nameof(credential), "Credential '{0}' does not exist or is disabled.", credentialAssignment.ConnectorIdentifier);
            Validator.IsTrue<ArgumentException>(credential.ChannelType == credentialAssignment.ChannelType, nameof(credentialAssignment.ChannelType), "Credential '{0}' is for channel type '{1}' but not '{2}'.", credentialAssignment.ConnectorIdentifier, credential.ChannelType.ToString(), credentialAssignment.ChannelType.ToString());

            await this.store.CreateOrUpdateCredentialAssignmentAsync(credentialAssignment);
        }

        public async Task<ConnectorCredentialAssignment> GetCredentialAssignmentByAccountAsync(string engagementAccount, ChannelType channelType)
        {
            var assignments = await this.store.ListCredentialAssignmentsByAccountAsync(engagementAccount, channelType, true);
            Validator.IsTrue<ResourceNotFoundException>(assignments != null && assignments.Count > 0, nameof(assignments), "No active credetial assignment for account '{0}' and channel '{1}'.", engagementAccount, channelType.ToString());

            return assignments.FirstOrDefault();
        }

        public async Task DeleteCredentialAssignmentsAsync(string engagementAccount, ConnectorIdentifier identifier)
        {
            var assignments = await this.store.ListCredentialAssignmentsByAccountAsync(engagementAccount, ChannelType.Both, false);
            Validator.IsTrue<ResourceNotFoundException>(assignments != null || assignments.Any(a => a.ConnectorIdentifier.Equals(identifier)), nameof(identifier), "The assignment does not exist.");

            await this.store.DeleteCredentialAssignmentsAsync(engagementAccount, identifier);
        }

        public async Task<List<ConnectorCredentialAssignment>> ListCredentialAssignmentsByAccountAsync(string engagementAccount, ChannelType channelType, bool activeOnly)
        {
            return await this.store.ListCredentialAssignmentsByAccountAsync(engagementAccount, channelType, activeOnly);
        }

        public async Task<List<ConnectorCredentialAssignment>> ListCredentialAssignmentsById(ConnectorIdentifier identifier, bool activeOnly = true)
        {
            return await this.store.ListCredentialAssignmentsById(identifier, activeOnly);
        }

        #endregion
    }
}
