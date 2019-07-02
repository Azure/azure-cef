// <copyright file="IEmailEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Engine
{
    public interface IEmailEngine : IDisposable
    {
        // Group
        Task<GroupCreateOrUpdateResult> CreateOrUpdateGroupAsync(Group group, string trackingId);

        Task<Group> GetGroupAsync(string account, string groupName, string continuationToken, int count, string trackingId);

        Task<GroupList> ListGroupsAsync(string account, DbContinuationToken continuationToken, int count, string trackingId);

        Task DeleteGroupAsync(string account, string groupName, string trackingId);

        Task DeleteGroupsAsync(string account, string trackingId);

        // SenderAddress
        Task<Sender> CreateOrUpdateSenderAsync(Sender sender, string trackingId);

        Task<Sender> GetSenderAsync(string account, Guid senderId, string trackingId);

        Task<SenderList> ListSendersAsync(string account, DbContinuationToken continuationToken, int count, string trackingId);

        Task DeleteSenderAsync(string account, Guid senderId, string trackingId);

        Task DeleteSendersAsync(string account, string trackingId);

        Task DeleteSendersbyDomainAsync(string account, string domain, string trackingId);

        // Template
        Task<Template> CreateOrUpdateTemplateAsync(Template template, string trackingId);

        Task<Template> GetTemplateAsync(string account, string template, string trackingId);

        Task<TemplateList> ListTemplatesAsync(string account, DbContinuationToken continuationToken, int count, string trackingId);

        Task DeleteTemplateAsync(string account, string template, string trackingId);

        Task DeleteTemplatesAsync(string account, string trackingId);

        Task DeleteTemplatesbyDomainAsync(string account, string domain, string trackingId);

        Task DeleteTemplatesbySenderAsync(string account, Guid senderId, string trackingId);

        Task UpdateTemplateStateByDomainAsync(string account, string domain, ResourceState fromState, ResourceState toState, string message = null, string trackingId = "");
    }
}
