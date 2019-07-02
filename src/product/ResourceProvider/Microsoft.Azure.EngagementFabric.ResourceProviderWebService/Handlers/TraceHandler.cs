// <copyright file="TraceHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Fabric;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Handlers
{
    internal class TraceHandler : DelegatingHandler
    {
        private readonly StatelessServiceContext serviceContext;

        public TraceHandler(StatelessServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid().ToString();

            ResourceProviderEventSource.Current.RequestReceived(
                requestId,
                request.RequestUri.PathAndQuery,
                request.Method.ToString());

            request.Headers.Add(Constants.OperationTrackingIdHeader, requestId);
            var result = await base.SendAsync(request, cancellationToken);

            ResourceProviderEventSource.Current.ResponseSent(
                requestId,
                result.StatusCode.ToString());

            return result;
        }
    }
}
