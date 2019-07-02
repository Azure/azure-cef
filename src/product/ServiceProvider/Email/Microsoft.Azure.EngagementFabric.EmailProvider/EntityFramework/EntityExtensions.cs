// -----------------------------------------------------------------------
// <copyright file="EntityExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Credential;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Newtonsoft.Json;
using Group = Microsoft.Azure.EngagementFabric.EmailProvider.Model.Group;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.EntityFramework
{
    public static class EntityExtensions
    {
        public static Account ToModel(this EngagementAccountEntity entity)
        {
            var account = new Account(entity.EngagementAccount);
            account.Properties = JsonConvert.DeserializeObject<PropertyCollection<string>>(entity.Properties);
            account.SubscriptionId = entity.SubscriptionId;

            return account;
        }

        public static Domain ToModel(this DomainEntity entity)
        {
            var domain = new Domain();
            domain.EngagementAccount = entity.EngagementAccount;
            domain.Value = entity.Domain;
            domain.Message = entity.Message;

            ResourceState state;
            if (Enum.TryParse(entity.State, out state))
            {
                domain.State = state;
            }
            else
            {
                EmailProviderEventSource.Current.Warning(EmailProviderEventSource.EmptyTrackingId, entity, nameof(ToModel), OperationStates.FailedMatch, $"Invalid Domain state. account={entity.EngagementAccount} Domain={entity.Domain} state={entity.State}");
            }

            return domain;
        }

        public static Group ToModel(this GroupEntity entity)
        {
            var group = new Group();
            group.EngagementAccount = entity.EngagementAccount;
            group.Name = entity.Name;
            group.Description = entity.Description;
            group.Properties = JsonConvert.DeserializeObject<PropertyCollection<string>>(entity.Properties);

            return group;
        }

        public static Sender ToModel(this SenderAddressEntity entity)
        {
            var sender = new Sender();
            sender.SenderAddrID = entity.Id.ToString();
            sender.SenderAddress = entity.Name;
            sender.ForwardAddress = entity.ForwardAddress;
            sender.EngagementAccount = entity.EngagementAccount;
            sender.Properties = JsonConvert.DeserializeObject<PropertyCollection<string>>(entity.Properties);

            return sender;
        }

        public static Template ToModel(this TemplateEntity entity)
        {
            var template = new Template();
            template.EngagementAccount = entity.EngagementAccount;
            template.Name = entity.Name;
            template.SenderId = entity.SenderAddressId;
            template.SenderAlias = entity.SenderAlias;
            template.Subject = entity.Subject;
            template.HtmlMsg = entity.MessageBody;
            template.EnableUnSubscribe = entity.EnableUnSubscribe;
            template.StateMessage = entity.StateMessage;
            ResourceState state;
            if (Enum.TryParse(entity.State, out state))
            {
                template.State = state;
            }
            else
            {
                EmailProviderEventSource.Current.Warning(EmailProviderEventSource.EmptyTrackingId, entity, nameof(ToModel), OperationStates.FailedMatch, $"Invalid template state. account={entity.EngagementAccount} template={entity.Name} state={entity.State}");
            }

            return template;
        }

        public static CredentialAssignment ToModel(this ConnectorCredentialAssignmentEntity entity)
        {
            var assignment = new CredentialAssignment();
            assignment.EngagementAccount = entity.EngagementAccount;
            assignment.Provider = entity.Provider;
            assignment.ConnectorId = entity.Id;
            assignment.Enabled = entity.Enabled;
            assignment.Active = entity.Active;

            return assignment;
        }

        public static ConnectorMetadata ToModel(this ConnectorMetadataEntity entity)
        {
            var metadata = new ConnectorMetadata();
            metadata.ConnectorName = entity.Provider;
            metadata.ConnectorUri = entity.ServiceUri;
            metadata.BatchSize = entity.BatchSize;
            return metadata;
        }
    }
}
