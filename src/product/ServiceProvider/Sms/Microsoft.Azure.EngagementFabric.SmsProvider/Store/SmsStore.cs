// <copyright file="SmsStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Microsoft.Azure.EngagementFabric.SmsProvider.EntityFramework;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Report;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Store
{
    public class SmsStore : ISmsStore
    {
        private readonly string connectionString;

        public SmsStore(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var entityStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                Metadata = "res://*/EntityFramework.SmsServiceDataModel.csdl|res://*/EntityFramework.SmsServiceDataModel.ssdl|res://*/EntityFramework.SmsServiceDataModel.msl",
                ProviderConnectionString = connectionStringBuilder.ToString()
            };

            this.connectionString = entityStringBuilder.ConnectionString;
        }

        #region Connector Metadata

        public async Task<ConnectorMetadata> GetConnectorMetadataAsync(string connectorName)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorMetadata.SingleOrDefaultAsync(c => c.Provider == connectorName);
                return entity?.ToModel();
            }
        }

        #endregion

        #region Connector Credential
        public async Task CreateOrUpdateCredentialAsync(SmsConnectorCredential credential)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorCredentials.SingleOrDefaultAsync(
                    c => c.Provider == credential.ConnectorName &&
                    c.Id == credential.ConnectorId);

                if (entity == null)
                {
                    entity = new ConnectorCredentialEntity();
                    entity.Provider = credential.ConnectorName;
                    entity.Id = credential.ConnectorId;
                    entity.ChannelType = credential.ChannelType.ToString();
                    entity.Data = JsonConvert.SerializeObject(credential);
                    entity.Created = entity.Modified = DateTime.UtcNow;
                    entity.Enabled = true;

                    ctx.ConnectorCredentials.Add(entity);
                }
                else
                {
                    entity.ChannelType = credential.ChannelType.ToString();
                    entity.Data = JsonConvert.SerializeObject(credential);
                    entity.Modified = DateTime.UtcNow;
                    entity.Enabled = true;
                }

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<SmsConnectorCredential> GetConnectorCredentialByIdAsync(ConnectorIdentifier identifier)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorCredentials.SingleOrDefaultAsync(
                    c => c.Provider == identifier.ConnectorName &&
                    c.Id == identifier.ConnectorId);

                return entity != null && entity.Enabled ? JsonConvert.DeserializeObject<SmsConnectorCredential>(entity.Data) : null;
            }
        }

        public async Task DeleteConnectorCredentialAsync(ConnectorIdentifier identifier)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorCredentials.SingleOrDefaultAsync(
                    c => c.Provider == identifier.ConnectorName &&
                    c.Id == identifier.ConnectorId);

                if (entity != null)
                {
                    ctx.ConnectorCredentials.Remove(entity);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region Connector Credential Assignment

        public async Task CreateOrUpdateCredentialAssignmentAsync(ConnectorCredentialAssignment credentialAssignment)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.ConnectorCredentialAssignments.Where(
                    c => c.EngagementAccount == credentialAssignment.EngagementAccount &&
                    c.ChannelType == credentialAssignment.ChannelType.ToString()).ToListAsync();

                var entity = entities?.SingleOrDefault(
                    e => e.Provider == credentialAssignment.ConnectorIdentifier.ConnectorName &&
                    e.Id == credentialAssignment.ConnectorIdentifier.ConnectorId);

                if (entity != null)
                {
                    entity.Enabled = credentialAssignment.Enabled;
                    entity.Active = credentialAssignment.Active;
                    entity.ExtendedCode = credentialAssignment.ExtendedCode;
                    entity.Modified = DateTime.UtcNow;
                }
                else
                {
                    entity = new ConnectorCredentialAssignmentEntity();
                    entity.EngagementAccount = credentialAssignment.EngagementAccount;
                    entity.ChannelType = credentialAssignment.ChannelType.ToString();
                    entity.Provider = credentialAssignment.ConnectorIdentifier.ConnectorName;
                    entity.Id = credentialAssignment.ConnectorIdentifier.ConnectorId;
                    entity.Enabled = credentialAssignment.Enabled;
                    entity.Active = credentialAssignment.Active;
                    entity.ExtendedCode = credentialAssignment.ExtendedCode;
                    entity.Created = entity.Modified = DateTime.UtcNow;

                    ctx.ConnectorCredentialAssignments.Add(entity);
                }

                // Make sure at most 1 active credential
                if (credentialAssignment.Active)
                {
                    foreach (var entry in entities.Where(e => e != entity))
                    {
                        entry.Active = false;
                    }
                }

                await ctx.SaveChangesAsync();
            }
        }

        public async Task DeleteCredentialAssignmentsAsync(string engagementAccount, ConnectorIdentifier identifier)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.ConnectorCredentialAssignments.Where(c => c.EngagementAccount == engagementAccount).ToListAsync();
                entities = entities.Where(e => identifier == null || (e.Provider.Equals(identifier.ConnectorName, StringComparison.OrdinalIgnoreCase) && e.Id.Equals(identifier.ConnectorId, StringComparison.OrdinalIgnoreCase))).ToList();

                if (entities != null)
                {
                    ctx.ConnectorCredentialAssignments.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task<List<ConnectorCredentialAssignment>> ListCredentialAssignmentsByAccountAsync(string engagementAccount, ChannelType channelType, bool activeOnly)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.ConnectorCredentialAssignments.Where(
                    c => c.EngagementAccount == engagementAccount &&
                    (channelType == ChannelType.Both || c.ChannelType == ChannelType.Both.ToString() || c.ChannelType == channelType.ToString()) &&
                    (!activeOnly || c.Active)).ToListAsync();

                return entities?.Select(e => e.ToModel()).ToList();
            }
        }

        public async Task<List<ConnectorCredentialAssignment>> ListCredentialAssignmentsById(ConnectorIdentifier identifier, bool activeOnly = true)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.ConnectorCredentialAssignments.Where(
                    c => c.Provider == identifier.ConnectorName &&
                    c.Id == identifier.ConnectorId &&
                    (!activeOnly || c.Active)).ToListAsync();

                return entities?.Select(e => e.ToModel()).ToList();
            }
        }

        #endregion

        #region Account

        public async Task<Account> CreateOrUpdateAccountAsync(Account account)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.EngagementAccounts.SingleOrDefaultAsync(a => a.EngagementAccount == account.EngagementAccount);
                if (entity == null)
                {
                    entity = new EngagementAccountEntity();
                    entity.EngagementAccount = account.EngagementAccount;
                    entity.Settings = JsonConvert.SerializeObject(account.AccountSettings);
                    entity.Created = entity.Modified = DateTime.UtcNow;
                    entity.SubscriptionId = account.SubscriptionId;
                    entity.Provider = account.Provider;

                    ctx.EngagementAccounts.Add(entity);
                }
                else
                {
                    entity.Settings = JsonConvert.SerializeObject(account.AccountSettings);
                    entity.SubscriptionId = account.SubscriptionId;
                    entity.Provider = account.Provider;
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return entity.ToModel();
            }
        }

        public async Task<Account> GetAccountAsync(string engagementAccount)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.EngagementAccounts.SingleOrDefaultAsync(a => a.EngagementAccount == engagementAccount);
                return entity?.ToModel();
            }
        }

        public async Task DeleteAccountAsync(string engagementAccount)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.EngagementAccounts.SingleOrDefaultAsync(a => a.EngagementAccount == engagementAccount);
                if (entity != null)
                {
                    ctx.EngagementAccounts.Remove(entity);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region Signature

        public async Task<Signature> CreateOrUpdateSignatureAsync(Signature signature)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Signatures.SingleOrDefaultAsync(
                    s => s.EngagementAccount == signature.EngagementAccount &&
                    s.Signature == signature.Value);

                if (entity == null)
                {
                    entity = new SignatureEntity();
                    entity.Signature = signature.Value;
                    entity.ChannelType = signature.ChannelType.ToString();
                    entity.EngagementAccount = signature.EngagementAccount;
                    entity.ExtendedCode = signature.ExtendedCode;
                    entity.State = signature.State.ToString();
                    entity.Message = signature.Message;
                    entity.Created = entity.Modified = DateTime.UtcNow;

                    ctx.Signatures.Add(entity);
                }
                else
                {
                    entity.ChannelType = signature.ChannelType.ToString();
                    entity.ExtendedCode = signature.ExtendedCode;
                    entity.State = signature.State.ToString();
                    entity.Message = signature.Message;
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return entity.ToModel();
            }
        }

        public async Task<Signature> GetSignatureAsync(string engagementAccount, string signature)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Signatures.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Signature == signature);

                return entity?.ToModel();
            }
        }

        public async Task<SignatureList> ListSignaturesAsync(string engagementAccount, DbContinuationToken continuationToken, int count)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var result = new SignatureList();
                var signagures = ctx.Signatures.Where(s => s.EngagementAccount == engagementAccount).OrderBy(s => s.Created);
                result.Total = signagures.Count();

                if (result.Total <= 0)
                {
                    return result;
                }

                var taken = count >= 0 ?
                    await signagures.Skip(continuationToken.Skip).Take(count).ToListAsync() :
                    await signagures.Skip(continuationToken.Skip).ToListAsync();

                result.Signatures = new List<Signature>();
                foreach (var entity in taken)
                {
                    result.Signatures.Add(entity.ToModel());
                }

                if (result.Total > continuationToken.Skip + count)
                {
                    result.NextLink = new DbContinuationToken(continuationToken.DatabaseId, continuationToken.Skip + count);
                }

                return result;
            }
        }

        public async Task DeleteSignatureAsync(string engagementAccount, string signature)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Signatures.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Signature == signature);

                if (entity != null)
                {
                    ctx.Signatures.Remove(entity);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteSignaturesAsync(string engagementAccount)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Signatures.Where(s => s.EngagementAccount == engagementAccount).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.Signatures.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region Template

        public async Task<Template> CreateOrUpdateTemplateAsync(Template template)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Templates.SingleOrDefaultAsync(
                    t => t.EngagementAccount == template.EngagementAccount &&
                    t.Name == template.Name);

                if (entity == null)
                {
                    entity = new TemplateEntity();
                    entity.Name = template.Name;
                    entity.EngagementAccount = template.EngagementAccount;
                    entity.Signature = template.Signature;
                    entity.Category = template.Category.ToString();
                    entity.Body = template.Body;
                    entity.State = template.State.ToString();
                    entity.Message = template.Message;
                    entity.Created = entity.Modified = DateTime.UtcNow;

                    ctx.Templates.Add(entity);
                }
                else
                {
                    entity.Signature = template.Signature;
                    entity.Category = template.Category.ToString();
                    entity.Body = template.Body;
                    entity.State = template.State.ToString();
                    entity.Message = template.Message;
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return entity.ToModel();
            }
        }

        public async Task<Template> GetTemplateAsync(string engagementAccount, string template)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Templates.SingleOrDefaultAsync(
                    t => t.EngagementAccount == engagementAccount &&
                    t.Name == template);

                return entity?.ToModel();
            }
        }

        public async Task<TemplateList> ListTemplatesAsync(string engagementAccount, DbContinuationToken continuationToken, int count)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var result = new TemplateList();
                var templates = ctx.Templates.Where(t => t.EngagementAccount == engagementAccount).OrderBy(t => t.Created);
                result.Total = templates.Count();

                if (result.Total <= 0)
                {
                    return result;
                }

                var taken = count >= 0 ?
                    await templates.Skip(continuationToken.Skip).Take(count).ToListAsync() :
                    await templates.Skip(continuationToken.Skip).ToListAsync();

                result.Templates = new List<Template>();
                foreach (var entity in taken)
                {
                    result.Templates.Add(entity.ToModel());
                }

                if (result.Total > continuationToken.Skip + count)
                {
                    result.NextLink = new DbContinuationToken(continuationToken.DatabaseId, continuationToken.Skip + count);
                }

                return result;
            }
        }

        public async Task DeleteTemplateAsync(string engagementAccount, string template)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Templates.SingleOrDefaultAsync(
                    t => t.EngagementAccount == engagementAccount &&
                    t.Name == template);

                if (entity != null)
                {
                    ctx.Templates.Remove(entity);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteTemplateBySignatureAsync(string engagementAccount, string signature)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Templates.Where(
                    t => t.EngagementAccount == engagementAccount &&
                    t.Signature == signature).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.Templates.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteTemplatesAsync(string engagementAccount)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Templates.Where(t => t.EngagementAccount == engagementAccount).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.Templates.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateTemplateStateBySignatureAsync(string engagementAccount, string signature, ResourceState fromState, ResourceState toState, string message = null)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Templates.Where(
                    t => t.EngagementAccount == engagementAccount &&
                    t.Signature == signature &&
                    t.State == fromState.ToString()).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    foreach (var entity in entities)
                    {
                        entity.State = toState.ToString();
                        entity.Message = message;
                    }

                    await ctx.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region Connector Agent Metadata

        public async Task CreateOrUpdateAgentMetadataAsync(AgentMetadata metadata)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorAgentMetadata.SingleOrDefaultAsync(
                    a => a.Provider == metadata.ConnectorName &&
                    a.Id == metadata.ConnectorId);

                if (entity == null)
                {
                    entity = new ConnectorAgentMetadataEntity();
                    entity.Provider = metadata.ConnectorName;
                    entity.Id = metadata.ConnectorId;
                    entity.LastMessageSendTime = metadata.LastMessageSendTime;
                    entity.LastReportUpdateTime = metadata.LastReportUpdateTime;
                    entity.PendingReceive = metadata.PendingReceive;
                    entity.Modified = DateTime.UtcNow;

                    ctx.ConnectorAgentMetadata.Add(entity);
                }
                else
                {
                    entity.LastMessageSendTime = metadata.LastMessageSendTime;
                    entity.LastReportUpdateTime = metadata.LastReportUpdateTime;
                    entity.PendingReceive = metadata.PendingReceive;
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<AgentMetadata> GetAgentMetadataAsync(ConnectorIdentifier identifier)
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorAgentMetadata.SingleOrDefaultAsync(
                    a => a.Provider == identifier.ConnectorName &&
                    a.Id == identifier.ConnectorId);

                return entity?.ToModel();
            }
        }

        public async Task<List<AgentMetadata>> ListAgentMetadataAsync()
        {
            using (var ctx = new SmsServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.ConnectorAgentMetadata.ToListAsync();

                var metaList = new List<AgentMetadata>();
                foreach (var entity in entities)
                {
                    metaList.Add(entity.ToModel());
                }

                return metaList;
            }
        }

        #endregion
    }
}
