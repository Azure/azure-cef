// <copyright file="IReportTelemetryManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Report
{
    using Report = Email.Common.Contract.Report;

    public interface IReportTelemetryManager
    {
        Task OnMessageSentAsync(InputMessage message, EmailMessageInfoExtension extension);

        Task OnMessageDispatchedAsync(OutputResult outputResult);

        Task<bool> OnReportUpdatedAsync(string engagementAccount, Report report);

        Task<string> AcquireNextAccountForPullingReportAsync(string processor);

        Task ReleaseAccountForPullingReportAsync(string processor, string engagementAccount);

        Task<List<MessageIdentifer>> ListInProgressMessagesAsync(string engagementAccount);

        Task<MessageRecord> GetMessageReportAsync(string engagementAccount, string messageId);

        Task OnAccountCreatedOrUpdatedAsync(string engagementAccount);

        Task OnAccountDeletedAsync(string engagementAccount);
    }
}
