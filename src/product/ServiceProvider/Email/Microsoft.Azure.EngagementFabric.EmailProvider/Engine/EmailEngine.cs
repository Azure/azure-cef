// <copyright file="EmailEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Configuration;
using Microsoft.Azure.EngagementFabric.EmailProvider.Credential;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.EmailProvider.Report;
using Microsoft.Azure.EngagementFabric.EmailProvider.Store;
using Contract = Microsoft.Azure.EngagementFabric.Email.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Engine
{
    public class EmailEngine : IEmailEngine
    {
        private IEmailStore store;
        private ICredentialManager credentialManager;
        private MetricManager metricManager;
        private ServiceConfiguration configuration;

        public EmailEngine(
            IEmailStoreFactory factory,
            ServiceConfiguration configuration,
            MetricManager metricManager,
            ICredentialManager credentialManager)
        {
            this.store = factory.GetStore();
            this.credentialManager = credentialManager;
            this.metricManager = metricManager;
            this.configuration = configuration;
        }

        public void Dispose()
        {
        }

        #region Group

        public async Task<Model.GroupCreateOrUpdateResult> CreateOrUpdateGroupAsync(Model.Group group, string trackingId)
        {
            try
            {
                var targets = group.Emails?.Count ?? 0;

                // Try to get existing group to get properties
                var existing = await store.GetGroupAsync(group.EngagementAccount, group.Name);
                if (existing != null)
                {
                    group.Properties = existing.Properties;
                }

                // Get Credential Contract
                var credential = await this.credentialManager.GetConnectorCredentialContractAsync(group.EngagementAccount);

                // Get EmailAccount
                var emailAccount = await this.store.GetEmailAccountAsync(group.EngagementAccount);

                // Request to Connector
                var connector = GetEmailEngineAgent(credential, group.EngagementAccount);
                var groupResult = await connector.CreateorUpdateGroupAsync(credential, emailAccount, group.ToContract(), CancellationToken.None);

                // Update store
                if (groupResult?.Group != null)
                {
                    group.Properties = groupResult.Group.Properties;
                    group = await this.store.CreateOrUpdateGroupAsync(group);
                }

                // Build result
                var state = groupResult != null && (groupResult.ErrorList == null || groupResult.ErrorList.Count() <= 0) ? GroupResultState.Updated :
                    groupResult != null && groupResult.ErrorList != null && groupResult.ErrorList.Count() != targets ? GroupResultState.PartiallyUpdated :
                    GroupResultState.NoUpdate;

                var invalid = groupResult?.ErrorList?.Select(e => new Model.GroupCreateOrUpdateResult.GroupCreateOrUpdateResultEntry
                {
                    Email = e.Email,
                    ErrorMessage = e.ErrorMessage
                }).ToList();

                var result = new Model.GroupCreateOrUpdateResult()
                {
                    Value = group.Name,
                    Description = group.Description,
                    State = state,
                    Invalid = invalid
                };

                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.CreateOrUpdateGroupAsync), OperationStates.Succeeded, $"account: {group.EngagementAccount}, group:{group.Name}");
                return result;
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.CreateOrUpdateGroupAsync), OperationStates.Failed, $"Failed to create email group for account: {group.EngagementAccount}", ex);
                throw new ApplicationException(string.Format($"Failed to create or update email group for account {group.EngagementAccount}"));
            }
        }

        public async Task<Model.Group> GetGroupAsync(string account, string groupName, string continuationToken, int count, string trackingId)
        {
            // Get Group
            var group = await this.store.GetGroupAsync(account, groupName);
            Validator.IsTrue<ResourceNotFoundException>(group != null, nameof(group), "Group '{0}' does not exist.", groupName);

            try
            {
                // Get Credential Contract
                var credential = await this.credentialManager.GetConnectorCredentialContractAsync(account);

                // Get EmailAccount
                var emailAccount = await this.store.GetEmailAccountAsync(account);

                // Request to Connector
                var connector = GetEmailEngineAgent(credential, account);
                var request = new GroupMemberRequest
                {
                    ContinuationToken = continuationToken,
                    Count = count
                };
                var result = await connector.GetGroupMembersAsync(credential, emailAccount, group.ToContract(), request, CancellationToken.None);
                group.Emails = result.Emails;
                group.NextLink = result.ContinuationToken;
                return group;
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.GetGroupAsync), OperationStates.Failed, $"Failed to get email group for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to get email group for account: {account}"));
            }
        }

        public async Task<GroupList> ListGroupsAsync(string account, DbContinuationToken continuationToken, int count, string trackingId)
        {
            try
            {
                var groups = await this.store.ListGroupsAsync(account, continuationToken, count);
                return groups;
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.ListGroupsAsync), OperationStates.Failed, $"Failed to get email group list for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to get email group list for account: {account}"));
            }
        }

        public async Task DeleteGroupAsync(string account, string groupName, string trackingId)
        {
            // Get Group
            var group = await this.store.GetGroupAsync(account, groupName);
            Validator.IsTrue<ResourceNotFoundException>(group != null, nameof(group), "Group '{0}' does not exist.", groupName);

            try
            {
                // Get Credential Contract
                var credential = await this.credentialManager.GetConnectorCredentialContractAsync(account);

                // Get EmailAccount
                var emailAccount = await this.store.GetEmailAccountAsync(account);

                // Request to Connector
                var connector = GetEmailEngineAgent(credential, account);
                await connector.DeleteGroupAsync(credential, emailAccount, new List<Contract.Group> { group.ToContract() }, CancellationToken.None);

                // Delete in Db
                await this.store.DeleteGroupAsync(account, groupName);

                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.DeleteGroupAsync), OperationStates.Succeeded, $"account: {account}, group: {groupName}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteGroupAsync), OperationStates.Failed, $"Failed to delete email group for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete email group for account: {account}"));
            }
        }

        public async Task DeleteGroupsAsync(string account, string trackingId)
        {
            // Get Group
            var groupList = await this.store.ListGroupsAsync(account, new DbContinuationToken(null), -1);
            if (groupList == null || groupList.Groups == null || groupList.Groups.Count() <= 0)
            {
                return;
            }

            try
            {
                // Get Credential Contract
                var credential = await this.credentialManager.GetConnectorCredentialContractAsync(account);

                // Get EmailAccount
                var emailAccount = await this.store.GetEmailAccountAsync(account);

                // Request to Connector
                var connector = GetEmailEngineAgent(credential, account);
                await connector.DeleteGroupAsync(credential, emailAccount, groupList.Groups.Select(g => g.ToContract()).ToList(), CancellationToken.None);

                // Delete in Db
                await this.store.DeleteGroupsAsync(account);

                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.DeleteGroupAsync), OperationStates.Succeeded, $"account: {account}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteGroupAsync), OperationStates.Failed, $"Failed to delete email groups for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete email groups for account: {account}"));
            }
        }

        #endregion

        #region SenderAddress
        public async Task<Sender> CreateOrUpdateSenderAsync(Sender sender, string trackingId)
        {
            // Check if domain exist
            var domain = await this.store.GetDomainAsync(sender.EngagementAccount, sender.SenderEmailAddress.Host);
            Validator.IsTrue<ArgumentException>(domain != null, nameof(domain), "Domain '{0}' does not exist.", sender.SenderEmailAddress.Host);

            // Try to get existing sender to get properties
            if (sender.SenderAddrID != null)
            {
                var existing = await store.GetSenderByIdAsync(sender.EngagementAccount, new Guid(sender.SenderAddrID));
                Validator.IsTrue<ArgumentException>(existing != null, nameof(existing), "SenderAddrID '{0}' does not exist.", sender.SenderAddrID);
                sender.Properties = existing.Properties;
            }
            else
            {
                var existing = await store.GetSenderByNameAsync(sender.EngagementAccount, sender.SenderAddress);
                Validator.IsTrue<ArgumentException>(existing == null, nameof(existing), "SenderAddress '{0}' already exists.", sender.SenderAddress);
            }

            try
            {
                // Get Credential Contract
                var credential = await this.credentialManager.GetConnectorCredentialContractAsync(sender.EngagementAccount);

                // Get EmailAccount
                var emailAccount = await this.store.GetEmailAccountAsync(sender.EngagementAccount);

                // Request to Connector
                var connector = GetEmailEngineAgent(credential, sender.EngagementAccount);
                var senderResult = await connector.CreateorUpdateSenderAddressAsync(credential, emailAccount, sender.ToContract(), CancellationToken.None);

                // Update store
                if (senderResult != null)
                {
                    sender.Properties = senderResult.Properties;
                    sender = await this.store.CreateOrUpdateSenderAsync(sender);
                }

                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.CreateOrUpdateSenderAsync), OperationStates.Succeeded, $"account: {sender.EngagementAccount} address:{sender.SenderEmailAddress.Address}");
                return sender;
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.CreateOrUpdateSenderAsync), OperationStates.Failed, $"Failed to create sender address for account: {sender.EngagementAccount}", ex);
                throw new ApplicationException(string.Format($"Failed to create or update sender address for account: {sender.EngagementAccount}"));
            }
        }

        public async Task<Sender> GetSenderAsync(string account, Guid senderId, string trackingId)
        {
            var senderResult = await this.store.GetSenderByIdAsync(account, senderId);
            Validator.IsTrue<ResourceNotFoundException>(senderResult != null, nameof(senderResult), "Sender address for '{0}' does not exist.", senderId);

            return senderResult;
        }

        public async Task<SenderList> ListSendersAsync(string account, DbContinuationToken continuationToken, int count, string trackingId)
        {
            try
            {
                var senders = await this.store.ListSendersAsync(account, continuationToken, count);
                return senders;
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.ListSendersAsync), OperationStates.Failed, $"Failed to get email sender address for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to get sender address list for account: {account}"));
            }
        }

        public async Task DeleteSenderAsync(string account, Guid senderId, string trackingId)
        {
            var sender = await this.store.GetSenderByIdAsync(account, senderId);
            Validator.IsTrue<ResourceNotFoundException>(sender != null, nameof(sender), "Sender address with Id '{0}' does not exist.", senderId);

            try
            {
                // Get Credential Contract
                var credential = await this.credentialManager.GetConnectorCredentialContractAsync(account);

                // Get EmailAccount
                var emailAccount = await this.store.GetEmailAccountAsync(account);

                // Request to Connector
                var connector = GetEmailEngineAgent(credential, account);
                await connector.DeleteSenderAddressAsync(credential, emailAccount, new List<SenderAddress> { sender.ToContract() }, CancellationToken.None);

                // Delete in Db
                await this.store.DeleteSenderAsync(account, senderId);

                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.DeleteSenderAsync), OperationStates.Succeeded, $"account: {account} sender: {senderId}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteSenderAsync), OperationStates.Failed, $"Failed to delete sender address for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete sender address for account: {account}"));
            }
        }

        public async Task DeleteSendersAsync(string account, string trackingId)
        {
            // Get Sender
            var senderList = await this.store.ListSendersAsync(account, new DbContinuationToken(null), -1);
            if (senderList == null || senderList.SenderAddresses == null || senderList.SenderAddresses.Count() <= 0)
            {
                return;
            }

            try
            {
                // Get Credential Contract
                var credential = await this.credentialManager.GetConnectorCredentialContractAsync(account);

                // Get EmailAccount
                var emailAccount = await this.store.GetEmailAccountAsync(account);

                // Request to Connector
                var connector = GetEmailEngineAgent(credential, account);
                await connector.DeleteSenderAddressAsync(credential, emailAccount, senderList.SenderAddresses.Select(s => s.ToContract()).ToList(), CancellationToken.None);

                // Delete in Db
                await this.store.DeleteSendersAsync(account);

                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.DeleteSendersAsync), OperationStates.Succeeded, $"account: {account}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteSendersAsync), OperationStates.Failed, $"Failed to delete sender addresses for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete sender addresses for account: {account}"));
            }
        }

        public async Task DeleteSendersbyDomainAsync(string account, string domain, string trackingId)
        {
            // Get Sender
            var senders = await this.store.GetSendersByDomainAsync(account, domain);
            if (senders == null || senders.Count() <= 0)
            {
                return;
            }

            try
            {
                // Get Credential Contract
                var credential = await this.credentialManager.GetConnectorCredentialContractAsync(account);

                // Get EmailAccount
                var emailAccount = await this.store.GetEmailAccountAsync(account);

                // Request to Connector
                var connector = GetEmailEngineAgent(credential, account);
                await connector.DeleteSenderAddressAsync(credential, emailAccount, senders.Select(s => s.ToContract()).ToList(), CancellationToken.None);

                // Delete in Db
                await this.store.DeleteSendersByDomainAsync(account, domain);

                EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteSendersAsync), OperationStates.Succeeded, $"account: {account}, domain:{domain}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteSendersAsync), OperationStates.Failed, $"Failed to delete sender addresses for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete sender addresses for account: {account}"));
            }
        }

        #endregion

        #region Templete
        public async Task<Template> CreateOrUpdateTemplateAsync(Template template, string trackingId)
        {
            // check if senderAddr exist
            var sender = await this.store.GetSenderByIdAsync(template.EngagementAccount, template.SenderId);
            Validator.IsTrue<ArgumentException>(sender != null, nameof(sender), "SenderAddrID '{0}' does not exist.", template.SenderId);

            try
            {
                var templateResult = await this.store.CreateOrUpdateTemplateAsync(template, sender);

                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.CreateOrUpdateGroupAsync), OperationStates.Succeeded, $"account: {template.EngagementAccount}, template: {template.Name}");
                return templateResult;
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.CreateOrUpdateSenderAsync), OperationStates.Failed, $"Failed to create template for account: {template.EngagementAccount}", ex);
                throw new ApplicationException(string.Format($"Failed to create or update template for account: {template.EngagementAccount}"));
            }
        }

        public async Task<Template> GetTemplateAsync(string account, string template, string trackingId)
        {
            var templateResult = await this.store.GetTemplateAsync(account, template);
            Validator.IsTrue<ResourceNotFoundException>(templateResult != null, nameof(templateResult), "template for '{0}' does not exist.", template);

            return templateResult;
        }

        public async Task<TemplateList> ListTemplatesAsync(string account, DbContinuationToken continuationToken, int count, string trackingId)
        {
            try
            {
                var templates = await this.store.ListTemplatesAsync(account, continuationToken, count);
                return templates;
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.ListTemplatesAsync), OperationStates.Failed, $"Failed to get email template for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to get email template list for account: {account}"));
            }
        }

        public async Task DeleteTemplateAsync(string account, string template, string trackingId)
        {
            var templateResult = await this.store.GetTemplateAsync(account, template);
            Validator.IsTrue<ResourceNotFoundException>(templateResult != null, nameof(templateResult), "Template for '{0}' does not exist.", template);

            try
            {
                await this.store.DeleteTemplateAsync(account, template);

                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.DeleteTemplateAsync), OperationStates.Succeeded, $"account: {account} template: {template}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteTemplateAsync), OperationStates.Failed, $"Failed to delete email template for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete email template for account: {account}"));
            }
        }

        public async Task DeleteTemplatesAsync(string account, string trackingId)
        {
            try
            {
                await this.store.DeleteTemplatesAsync(account);
                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.DeleteTemplatesAsync), OperationStates.Succeeded, $"account: {account}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteTemplatesAsync), OperationStates.Failed, $"Failed to delete email template for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete email template for account: {account}"));
            }
        }

        public async Task DeleteTemplatesbyDomainAsync(string account, string domain, string trackingId)
        {
            try
            {
                await this.store.DeleteTemplatesByDomainAsync(account, domain);
                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.DeleteTemplatesbyDomainAsync), OperationStates.Succeeded, $"account: {account}, domain: {domain}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteTemplatesbyDomainAsync), OperationStates.Failed, $"Failed to delete email templates for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete email templates for account: {account}"));
            }
        }

        public async Task DeleteTemplatesbySenderAsync(string account, Guid senderId, string trackingId)
        {
            try
            {
                await this.store.DeleteTemplatesBySenderAsync(account, senderId);
                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.DeleteTemplatesbyDomainAsync), OperationStates.Succeeded, $"account: {account}, sender: {senderId}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.DeleteTemplatesbyDomainAsync), OperationStates.Failed, $"Failed to delete email templates for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to delete email templates for account: {account}"));
            }
        }

        public async Task UpdateTemplateStateByDomainAsync(string account, string domain, ResourceState fromState, ResourceState toState, string message = null, string trackingId = "")
        {
            try
            {
                await this.store.UpdateTemplateStateByDomainAsync(account, domain, fromState, toState, message);
                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.UpdateTemplateStateByDomainAsync), OperationStates.Succeeded, $"account: {account} domain: {domain} from: {fromState.ToString()} to: {toState.ToString()}");
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.UpdateTemplateStateByDomainAsync), OperationStates.Failed, $"Failed to update email template state for account: {account}", ex);
                throw new ApplicationException(string.Format($"Failed to update email template state for account: {account}"));
            }
        }

        #endregion
        private EmailEngineAgent GetEmailEngineAgent(ConnectorCredential credential, string engagementAccount)
        {
            // Get agent
            return new EmailEngineAgent(credential, engagementAccount, this.configuration);
        }
    }
}