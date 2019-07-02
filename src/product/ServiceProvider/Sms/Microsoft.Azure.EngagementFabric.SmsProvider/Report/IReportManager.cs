// <copyright file="IReportManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Report
{
    public interface IReportManager : IDisposable
    {
        Task<MessageDetails> GetMessageAsync(string engagementAccount, string messageId, int count, TableContinuationToken continuationToken);

        Task<PerMessageAggregationList> GetPerMessageAggregationAsync(
            string engagementAccount,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int count,
            TableContinuationToken continuationToken);

        Task<PerPeriodAggregationList> GetPerPeriodAggregationAsync(
            string requestId,
            string engagementAccount,
            long startTime,
            long endTime,
            int seriesResolutionInMinutes);

        Task OnMessageSentAsync(string engagementAccount, InputMessage message, SmsMessageInfoExtension extension);

        Task OnDispatchCompleteAsync(OutputResult outputResult);

        Task OnReportPulledAsync(ReportAgent agent, List<ReportDetail> reports);

        Task<int> OnReportPushedAsync(List<ReportDetail> reports, ConnectorIdentifier identifier);

        Task OnAccountCreatedOrUpdatedAsync(string engagementAccount);

        Task OnAccountDeletedAsync(string engagementAccount);

        void OnAgentUnSubscribed(ReportAgent agent);
    }
}
