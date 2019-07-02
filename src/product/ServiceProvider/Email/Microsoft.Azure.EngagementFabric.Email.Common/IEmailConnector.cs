// <copyright file="IEmailConnector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.Email.Common
{
    public interface IEmailConnector : IDispatcherConnector
    {
        // Account
        Task<EmailAccount> CreateEmailAccountAsync(ConnectorCredential credential, EmailAccount emailAccount, CancellationToken cancellationToken);

        Task DeleteEmailAccountAsync(ConnectorCredential credential, EmailAccount emailAccount, CancellationToken cancellationToken);

        // SenderAddress
        Task<SenderAddress> CreateorUpdateSenderAddressAsync(ConnectorCredential credential, EmailAccount emailAccount, SenderAddress senderAddress, CancellationToken cancellationToken);

        Task DeleteSenderAddressAsync(ConnectorCredential credential, EmailAccount emailAccount, List<SenderAddress> senderAddressList, CancellationToken cancellationToken);

        // Group
        Task<GroupCreateOrUpdateResult> CreateorUpdateGroupAsync(ConnectorCredential credential, EmailAccount emailAccount, Group group, CancellationToken cancellationToken);

        Task<GroupMembers> GetGroupMembersAsync(ConnectorCredential credential, EmailAccount emailAccount, Group group, GroupMemberRequest request, CancellationToken cancellationToken);

        Task DeleteGroupAsync(ConnectorCredential credential, EmailAccount emailAccount, List<Group> groupList, CancellationToken cancellationToken);

        // Mailing
        Task<ReportList> GetMailReportAsync(ConnectorCredential credential, EmailAccount emailAccount, List<MessageIdentifer> messageIdentifers, CancellationToken cancellationToken);

        Task DeleteMailingAsync(ConnectorCredential credential, EmailAccount emailAccount, List<MessageIdentifer> messageIdentifers, CancellationToken cancellationToken);
    }
}
