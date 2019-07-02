// <copyright file="IInboundTelemetryManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Inbound
{
    public interface IInboundTelemetryManager
    {
        Task InsertInboundMessagesAsync(Signature signature, List<InboundMessage> messages, string extendedCode);

        Task<List<InboundMessageTableEntity>> GetMessagesAsync(string engagementAccount, DateTime startTime, int count);

        Task DeleteMessagesAsync(List<InboundMessageTableEntity> messages);

        Task DeleteMessagesAsync(string engagementAccount);

        Task CleanupMessageAsync(DateTime before);
    }
}
