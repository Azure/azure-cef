// <copyright file="IInboundManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Inbound
{
    public interface IInboundManager : IDisposable
    {
        Task<HttpResponseMessage> OnInboundMessageReceived(string connectorName, HttpRequestMessage request, CancellationToken cancellationToken);

        Task<List<InboundMessageDetail>> GetInboundMessages(string engagementAccount);

        Task OnAccountDeletedAsync(string engagementAccount);
    }
}
