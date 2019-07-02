// <copyright file="EmailStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Credential;
using Microsoft.Azure.EngagementFabric.EmailProvider.EntityFramework;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Newtonsoft.Json;
using Group = Microsoft.Azure.EngagementFabric.EmailProvider.Model.Group;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Store
{
    public class EmailStore : IEmailStore
    {
        private readonly string connectionString;

        public EmailStore(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var entityStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                Metadata = "res://*/EntityFramework.EmailServiceDataModel.csdl|res://*/EntityFramework.EmailServiceDataModel.ssdl|res://*/EntityFramework.EmailServiceDataModel.msl",
                ProviderConnectionString = connectionStringBuilder.ToString()
            };

            this.connectionString = entityStringBuilder.ConnectionString;
        }

        #region Connector Metadata

        public async Task<ConnectorMetadata> GetConnectorMetadataAsync(string connectorName)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorMetadata.SingleOrDefaultAsync(c => c.Provider == connectorName);
                return entity?.ToModel();
            }
        }

        #endregion

        #region Connector Credential
        public async Task CreateOrUpdateCredentialAsync(EmailConnectorCredential credential)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorCredentials.SingleOrDefaultAsync(
                    c => c.Provider == credential.ConnectorName &&
                    c.Id == credential.ConnectorId);

                if (entity == null)
                {
                    entity = new ConnectorCredentialEntity();
                    entity.Provider = credential.ConnectorName;
                    entity.Id = credential.ConnectorId;
                    entity.Data = JsonConvert.SerializeObject(credential);
                    entity.Created = entity.Modified = DateTime.UtcNow;
                    entity.Enabled = true;

                    ctx.ConnectorCredentials.Add(entity);
                }
                else
                {
                    entity.Data = JsonConvert.SerializeObject(credential);
                    entity.Modified = DateTime.UtcNow;
                    entity.Enabled = true;
                }

                await ctx.SaveChangesAsync();
            }
        }

        public async Task<EmailConnectorCredential> GetConnectorCredentialByIdAsync(ConnectorIdentifier identifier)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.ConnectorCredentials.SingleOrDefaultAsync(
                    c => c.Provider == identifier.ConnectorName &&
                    c.Id == identifier.ConnectorId);

                return entity != null && entity.Enabled ? JsonConvert.DeserializeObject<EmailConnectorCredential>(entity.Data) : null;
            }
        }

        public async Task DeleteConnectorCredentialAsync(ConnectorIdentifier identifier)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
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

        public async Task CreateOrUpdateCredentialAssignmentAsync(CredentialAssignment credentialAssignment)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.ConnectorCredentialAssignments.Where(
                    c => c.EngagementAccount == credentialAssignment.EngagementAccount).ToListAsync();

                var entity = entities?.SingleOrDefault(
                    e => e.Provider == credentialAssignment.Provider &&
                    e.Id == credentialAssignment.ConnectorId);

                if (entity != null)
                {
                    entity.Enabled = credentialAssignment.Enabled;
                    entity.Active = credentialAssignment.Active;
                    entity.Modified = DateTime.UtcNow;
                }
                else
                {
                    entity = new ConnectorCredentialAssignmentEntity();
                    entity.EngagementAccount = credentialAssignment.EngagementAccount;
                    entity.Provider = credentialAssignment.Provider;
                    entity.Id = credentialAssignment.ConnectorId;
                    entity.Enabled = credentialAssignment.Enabled;
                    entity.Active = credentialAssignment.Active;
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
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
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

        public async Task<List<CredentialAssignment>> ListCredentialAssignmentsByAccountAsync(string engagementAccount, bool activeOnly)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.ConnectorCredentialAssignments.Where(
                    c => c.EngagementAccount == engagementAccount &&
                    (!activeOnly || c.Active)).ToListAsync();

                return entities?.Select(e => e.ToModel()).ToList();
            }
        }

        public async Task<List<CredentialAssignment>> ListCredentialAssignmentsById(ConnectorIdentifier identifier, bool activeOnly = true)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
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
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.EngagementAccounts.SingleOrDefaultAsync(a => a.EngagementAccount == account.EngagementAccount);
                if (entity == null)
                {
                    entity = new EngagementAccountEntity();
                    entity.EngagementAccount = account.EngagementAccount;
                    entity.Properties = JsonConvert.SerializeObject(account.Properties);
                    entity.Created = entity.Modified = DateTime.UtcNow;
                    entity.SubscriptionId = account.SubscriptionId;

                    ctx.EngagementAccounts.Add(entity);
                }
                else
                {
                    entity.Properties = JsonConvert.SerializeObject(account.Properties);
                    entity.SubscriptionId = account.SubscriptionId;
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return entity.ToModel();
            }
        }

        public async Task<Account> GetAccountAsync(string engagementAccount)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.EngagementAccounts.SingleOrDefaultAsync(a => a.EngagementAccount == engagementAccount);
                return entity?.ToModel();
            }
        }

        public async Task DeleteAccountAsync(string engagementAccount)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
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

        #region Domain

        public async Task<Domain> CreateOrUpdateDomainAsync(Domain domain)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Domains.SingleOrDefaultAsync(
                    s => s.EngagementAccount == domain.EngagementAccount &&
                    s.Domain == domain.Value);

                if (entity == null)
                {
                    entity = new DomainEntity();
                    entity.Domain = domain.Value;
                    entity.EngagementAccount = domain.EngagementAccount;
                    entity.State = domain.State.ToString();
                    entity.Message = domain.Message;
                    entity.Created = entity.Modified = DateTime.UtcNow;

                    ctx.Domains.Add(entity);
                }
                else
                {
                    entity.State = domain.State.ToString();
                    entity.Message = domain.Message;
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return entity.ToModel();
            }
        }

        public async Task<List<Domain>> GetDomainsByNameAsync(string domain)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Domains.Where(s => s.Domain == domain).ToListAsync();
                return entities?.Select(e => e.ToModel()).ToList();
            }
        }

        public async Task<Domain> GetDomainAsync(string engagementAccount, string domain)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Domains.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Domain == domain);

                return entity?.ToModel();
            }
        }

        public async Task<DomainList> ListDomainsByAccountAsync(string engagementAccount, DbContinuationToken continuationToken, int count)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var result = new DomainList();
                var domains = ctx.Domains.Where(s => s.EngagementAccount == engagementAccount).OrderBy(s => s.Created);
                result.Total = domains.Count();

                if (result.Total <= 0)
                {
                    return result;
                }

                var taken = count >= 0 ?
                    await domains.Skip(continuationToken.Skip).Take(count).ToListAsync() :
                    await domains.Skip(continuationToken.Skip).ToListAsync();

                result.Domains = new List<Domain>();
                foreach (var entity in taken)
                {
                    result.Domains.Add(entity.ToModel());
                }

                if (result.Total > continuationToken.Skip + count)
                {
                    result.NextLink = new DbContinuationToken(continuationToken.DatabaseId, continuationToken.Skip + count);
                }

                return result;
            }
        }

        public async Task DeleteDomainAsync(string engagementAccount, string domain)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Domains.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Domain == domain);

                if (entity != null)
                {
                    ctx.Domains.Remove(entity);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteDomainsAsync(string engagementAccount)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Domains.Where(s => s.EngagementAccount == engagementAccount).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.Domains.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region Group
        public async Task<Group> CreateOrUpdateGroupAsync(Group group)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Groups.SingleOrDefaultAsync(
                    s => s.EngagementAccount == group.EngagementAccount &&
                    s.Name == group.Name);

                if (entity == null)
                {
                    entity = new GroupEntity();
                    entity.Name = group.Name;
                    entity.EngagementAccount = group.EngagementAccount;
                    entity.Description = group.Description;
                    entity.Properties = JsonConvert.SerializeObject(group.Properties);
                    entity.Created = entity.Modified = DateTime.UtcNow;

                    ctx.Groups.Add(entity);
                }
                else
                {
                    entity.Description = group.Description;
                    entity.Properties = JsonConvert.SerializeObject(group.Properties);
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return entity.ToModel();
            }
        }

        public async Task<Group> GetGroupAsync(string engagementAccount, string group)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Groups.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Name == group);

                return entity?.ToModel();
            }
        }

        public async Task<GroupList> ListGroupsAsync(string engagementAccount, DbContinuationToken continuationToken, int count)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var result = new GroupList();
                var groups = ctx.Groups.Where(s => s.EngagementAccount == engagementAccount).OrderBy(s => s.Created);
                result.Total = groups.Count();
                if (result.Total <= 0)
                {
                    return result;
                }

                var taken = count >= 0 ?
                   await groups.Skip(continuationToken.Skip).Take(count).ToListAsync() :
                   await groups.Skip(continuationToken.Skip).ToListAsync();

                result.Groups = new List<Group>();
                foreach (var entity in taken)
                {
                    result.Groups.Add(entity.ToModel());
                }

                if (result.Total > continuationToken.Skip + count)
                {
                    result.NextLink = new DbContinuationToken(continuationToken.DatabaseId, continuationToken.Skip + count);
                }

                return result;
            }
        }

        public async Task DeleteGroupAsync(string engagementAccount, string group)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Groups.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Name == group);

                if (entity != null)
                {
                    ctx.Groups.Remove(entity);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteGroupsAsync(string engagementAccount)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Groups.Where(s => s.EngagementAccount == engagementAccount).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.Groups.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region Sender
        public async Task<Sender> CreateOrUpdateSenderAsync(Sender sender)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = sender.SenderAddrID != null ? await ctx.SenderAddresses.SingleOrDefaultAsync(
                    s => s.EngagementAccount == sender.EngagementAccount &&
                    s.Id == new Guid(sender.SenderAddrID)) : null;

                if (entity == null)
                {
                    entity = new SenderAddressEntity();
                    entity.Id = Guid.NewGuid();
                    entity.Name = sender.SenderEmailAddress.Address;
                    entity.Domain = sender.SenderEmailAddress.Host;
                    entity.EngagementAccount = sender.EngagementAccount;
                    entity.ForwardAddress = sender.ForwardEmailAddress.Address;
                    entity.Properties = JsonConvert.SerializeObject(sender.Properties);
                    entity.Created = entity.Modified = DateTime.UtcNow;

                    ctx.SenderAddresses.Add(entity);
                }
                else
                {
                    entity.Name = sender.SenderEmailAddress.Address;
                    entity.Domain = sender.SenderEmailAddress.Host;
                    entity.ForwardAddress = sender.ForwardEmailAddress.Address;
                    entity.Properties = JsonConvert.SerializeObject(sender.Properties);
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return entity.ToModel();
            }
        }

        public async Task<Sender> GetSenderByIdAsync(string engagementAccount, Guid senderId)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.SenderAddresses.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Id == senderId);

                return entity?.ToModel();
            }
        }

        public async Task<Sender> GetSenderByNameAsync(string engagementAccount, string senderAddr)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.SenderAddresses.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Name == senderAddr);

                return entity?.ToModel();
            }
        }

        public async Task<SenderList> ListSendersAsync(string engagementAccount, DbContinuationToken continuationToken, int count)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var result = new SenderList();
                var senders = ctx.SenderAddresses.Where(s => s.EngagementAccount == engagementAccount).OrderBy(s => s.Created);
                result.Total = senders.Count();
                if (result.Total <= 0)
                {
                    return result;
                }

                var taken = count >= 0 ?
                   await senders.Skip(continuationToken.Skip).Take(count).ToListAsync() :
                   await senders.Skip(continuationToken.Skip).ToListAsync();

                result.SenderAddresses = new List<Sender>();
                foreach (var entity in taken)
                {
                    result.SenderAddresses.Add(entity.ToModel());
                }

                if (result.Total > continuationToken.Skip + count)
                {
                    result.NextLink = new DbContinuationToken(continuationToken.DatabaseId, continuationToken.Skip + count);
                }

                return result;
            }
        }

        public async Task DeleteSenderAsync(string engagementAccount, Guid senderId)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.SenderAddresses.SingleOrDefaultAsync(
                    s => s.EngagementAccount == engagementAccount &&
                     s.Id == senderId);

                if (entity != null)
                {
                    ctx.SenderAddresses.Remove(entity);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteSendersAsync(string engagementAccount)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.SenderAddresses.Where(s => s.EngagementAccount == engagementAccount).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.SenderAddresses.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Sender>> GetSendersByDomainAsync(string engagementAccount, string domain)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.SenderAddresses.Where(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Domain == domain).ToListAsync();

                return entities?.Select(e => e.ToModel()).ToList();
            }
        }

        public async Task DeleteSendersByDomainAsync(string engagementAccount, string domain)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.SenderAddresses.Where(
                    s => s.EngagementAccount == engagementAccount &&
                    s.Domain == domain).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.SenderAddresses.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }
        #endregion

        #region Template

        public async Task<Template> CreateOrUpdateTemplateAsync(Template template, Sender sender)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Templates.SingleOrDefaultAsync(
                    t => t.EngagementAccount == template.EngagementAccount &&
                    t.Name == template.Name);

                if (entity == null)
                {
                    entity = new TemplateEntity();
                    entity.Name = template.Name;
                    entity.EngagementAccount = template.EngagementAccount;
                    entity.Domain = sender.Domain;
                    entity.SenderAddressId = template.SenderId;
                    entity.SenderAlias = template.SenderAlias;
                    entity.ReplyAddress = string.Empty;
                    entity.Subject = template.Subject;
                    entity.MessageBody = template.HtmlMsg;
                    entity.EnableUnSubscribe = template.EnableUnSubscribe;
                    entity.State = template.State.ToString();
                    entity.StateMessage = template.StateMessage;
                    entity.Created = entity.Modified = DateTime.UtcNow;

                    ctx.Templates.Add(entity);
                }
                else
                {
                    entity.Domain = sender.Domain;
                    entity.SenderAddressId = template.SenderId;
                    entity.SenderAlias = template.SenderAlias;
                    entity.ReplyAddress = string.Empty;
                    entity.Subject = template.Subject;
                    entity.MessageBody = template.HtmlMsg;
                    entity.EnableUnSubscribe = template.EnableUnSubscribe;
                    entity.State = template.State.ToString();
                    entity.StateMessage = template.StateMessage;
                    entity.Modified = DateTime.UtcNow;
                }

                await ctx.SaveChangesAsync();
                return entity.ToModel();
            }
        }

        public async Task<Template> GetTemplateAsync(string engagementAccount, string template)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entity = await ctx.Templates.SingleOrDefaultAsync(
                    t => t.EngagementAccount == engagementAccount &&
                    t.Name == template);

                return entity?.ToModel();
            }
        }

        public async Task<TemplateList> ListTemplatesAsync(string engagementAccount, DbContinuationToken continuationToken, int count)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
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
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
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

        public async Task DeleteTemplatesAsync(string engagementAccount)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Templates.Where(
                    t => t.EngagementAccount == engagementAccount).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.Templates.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteTemplatesByDomainAsync(string engagementAccount, string domain)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Templates.Where(
                    t => t.EngagementAccount == engagementAccount &&
                    t.Domain == domain).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.Templates.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteTemplatesBySenderAsync(string engagementAccount, Guid senderId)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Templates.Where(
                    t => t.EngagementAccount == engagementAccount &&
                    t.SenderAddressId == senderId).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    ctx.Templates.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateTemplateStateByDomainAsync(string account, string domain, ResourceState fromState, ResourceState toState, string message = null)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var entities = await ctx.Templates.Where(
                    t => t.EngagementAccount == account &&
                    t.Domain == domain &&
                    t.State == fromState.ToString()).ToListAsync();

                if (entities != null && entities.Count > 0)
                {
                    foreach (var entity in entities)
                    {
                        entity.State = toState.ToString();
                        entity.StateMessage = message;
                    }

                    await ctx.SaveChangesAsync();
                }
            }
        }

        #endregion

        #region EmailAccount

        public async Task<EmailAccount> GetEmailAccountAsync(string engagementAccount)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var account = await ctx.EngagementAccounts.SingleOrDefaultAsync(a => a.EngagementAccount == engagementAccount);
                if (account == null)
                {
                    return null;
                }

                var domains = await ctx.Domains.Where(
                    d => d.EngagementAccount == engagementAccount &&
                    d.State == ResourceState.Active.ToString()).OrderBy(s => s.Created).ToListAsync();

                return new EmailAccount
                {
                    EngagementAccount = engagementAccount,
                    Domains = domains?.Select(d => d.Domain).ToList(),
                    Properties = JsonConvert.DeserializeObject<PropertyCollection<string>>(account.Properties)
                };
            }
        }

        public async Task<EmailAccount> UpdateEmailAccountAsync(EmailAccount emailAccount)
        {
            using (var ctx = new EmailServiceDbEntities(this.connectionString))
            {
                var account = await ctx.EngagementAccounts.SingleOrDefaultAsync(a => a.EngagementAccount == emailAccount.EngagementAccount);
                if (account == null)
                {
                    return null;
                }

                account.Properties = JsonConvert.SerializeObject(emailAccount.Properties);
                account.Modified = DateTime.UtcNow;
                await ctx.SaveChangesAsync();

                return emailAccount;
            }
        }

        #endregion
    }
}
