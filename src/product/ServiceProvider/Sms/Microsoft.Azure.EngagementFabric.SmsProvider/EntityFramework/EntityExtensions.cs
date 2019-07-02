// -----------------------------------------------------------------------
// <copyright file="EntityExtensions.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Report;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.EntityFramework
{
    public static class EntityExtensions
    {
        public static Account ToModel(this EngagementAccountEntity entity)
        {
            var account = new Account();
            account.EngagementAccount = entity.EngagementAccount;
            account.AccountSettings = JsonConvert.DeserializeObject<AccountSettings>(entity.Settings);
            account.SubscriptionId = entity.SubscriptionId;
            account.Provider = entity.Provider;

            return account;
        }

        public static Signature ToModel(this SignatureEntity entity)
        {
            var signature = new Signature();
            signature.EngagementAccount = entity.EngagementAccount;
            signature.Value = entity.Signature;
            signature.Message = entity.Message;
            signature.ExtendedCode = entity.ExtendedCode;

            ResourceState state;
            if (Enum.TryParse(entity.State, out state))
            {
                signature.State = state;
            }
            else
            {
                SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, entity, nameof(ToModel), OperationStates.FailedMatch, $"Invalid signature state. account={entity.EngagementAccount} signature={entity.Signature} state={entity.State}");
            }

            ChannelType channelType;
            if (Enum.TryParse(entity.ChannelType, out channelType))
            {
                signature.ChannelType = channelType;
            }
            else
            {
                SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, entity, nameof(ToModel), OperationStates.FailedMatch, $"Invalid signature channel type. account={entity.EngagementAccount} signature={entity.Signature} channelType={entity.ChannelType}");
            }

            return signature;
        }

        public static Template ToModel(this TemplateEntity entity)
        {
            var template = new Template();
            template.EngagementAccount = entity.EngagementAccount;
            template.Name = entity.Name;
            template.Signature = entity.Signature;
            template.Body = entity.Body;
            template.Message = entity.Message;

            MessageCategory category;
            if (Enum.TryParse(entity.Category, out category))
            {
                template.Category = category;
            }
            else
            {
                SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, entity, nameof(ToModel), OperationStates.FailedMatch, $"Invalid template category. account={entity.EngagementAccount} template={entity.Name} state={entity.State}");
            }

            ResourceState state;
            if (Enum.TryParse(entity.State, out state))
            {
                template.State = state;
            }
            else
            {
                SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, entity, nameof(ToModel), OperationStates.FailedMatch, $"Invalid template state. account={entity.EngagementAccount} template={entity.Name} state={entity.State}");
            }

            return template;
        }

        public static AgentMetadata ToModel(this ConnectorAgentMetadataEntity entity)
        {
            var meta = new AgentMetadata();
            meta.ConnectorName = entity.Provider;
            meta.ConnectorId = entity.Id;
            meta.LastMessageSendTime = entity.LastMessageSendTime;
            meta.LastReportUpdateTime = entity.LastReportUpdateTime;
            meta.PendingReceive = entity.PendingReceive ?? 0;

            return meta;
        }

        public static ConnectorCredentialAssignment ToModel(this ConnectorCredentialAssignmentEntity entity)
        {
            var assignment = new ConnectorCredentialAssignment();
            assignment.EngagementAccount = entity.EngagementAccount;
            assignment.ConnectorIdentifier = new ConnectorIdentifier(entity.Provider, entity.Id);
            assignment.Enabled = entity.Enabled;
            assignment.Active = entity.Active;
            assignment.ExtendedCode = entity.ExtendedCode;

            ChannelType channelType;
            if (Enum.TryParse(entity.ChannelType, out channelType))
            {
                assignment.ChannelType = channelType;
            }

            return assignment;
        }

        public static ConnectorMetadata ToModel(this ConnectorMetadataEntity entity)
        {
            var metadata = new ConnectorMetadata();
            metadata.ConnectorName = entity.Provider;
            metadata.ConnectorUri = entity.ServiceUri;
            metadata.BatchSize = entity.BatchSize;
            metadata.SingleReportForLongMessage = entity.SingleReportForLongMessage;

            if (Enum.TryParse(entity.ReportType, out ConnectorMetadata.ConnectorInboundType reportType))
            {
                metadata.ReportType = reportType;
            }

            if (Enum.TryParse(entity.ReportType, out ConnectorMetadata.ConnectorInboundType moType))
            {
                metadata.InboundMessageType = moType;
            }

            return metadata;
        }
    }
}
