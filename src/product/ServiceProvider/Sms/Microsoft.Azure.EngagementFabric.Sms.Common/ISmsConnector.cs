// <copyright file="ISmsConnector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.Sms.Common
{
    public interface ISmsConnector : IDispatcherConnector
    {
        Task<ReportResponse> FetchMessageReportsAsync(ConnectorCredential credential, CancellationToken cancellationToken);

        Task<string> ParseConnectorIdFromInboundMessageAsync(InboundHttpRequestMessage request, CancellationToken cancellationToken);

        Task<InboundResponse> ParseInboundRequestAsync(InboundHttpRequestMessage request, ConnectorCredential credential, CancellationToken cancellationToken);

        Task<List<string>> ParseExtendedCodeAsync(string extendedCode, List<int> segmentLengths, CancellationToken cancellationToken);
    }
}
