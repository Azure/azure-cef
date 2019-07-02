// <copyright file="IReportTelemetryManager.cs" company="Microsoft Corporation">
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
    public interface IReportTelemetryManager
    {
        Task OnMessageSentAsync(InputMessage message, SmsMessageInfoExtension extension);

        Task<bool> OnMessageDispatchedAsync(string engagementAccount, string messageId, string customMessageId, List<string> targets, ConnectorIdentifier connector);

        Task<bool> OnMessageReportUpdatedAsync(string messageId, string customMessageId, List<ReportDetail> reports, ConnectorIdentifier connector);

        Task<MessageDetails> GetMessageHistoryAsync(string engagementAccount, string messageId, int count, TableContinuationToken continuationToken);

        Task<PerMessageAggregationList> GetPerMessageAggregationAsync(
            string engagementAccount,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            int count,
            TableContinuationToken continuationToken);

        Task CreateMessageHistoryIfNotExistAsync(string engagementAccount);

        Task DeleteMessageHistoryAsync(string engagementAccount);

        // Messgae batch table for debug
        Task InsertMessageBatchRecordAsync(OutputResult result);
    }
}
