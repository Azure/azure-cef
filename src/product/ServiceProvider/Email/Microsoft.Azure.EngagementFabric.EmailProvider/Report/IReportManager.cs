// <copyright file="IReportManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    public interface IReportManager
    {
        Task OnMessageSentAsync(string engagementAccount, InputMessage message, EmailMessageInfoExtension extension);

        Task OnDispatchCompleteAsync(OutputResult outputResult);

        Task PullReportsAsync();

        Task<MessageRecord> GetReportAsync(string engagementAccount, string messageId);

        Task OnAccountCreatedOrUpdatedAsync(string engagementAccount);

        Task OnAccountDeletedAsync(string engagementAccount);
    }
}
