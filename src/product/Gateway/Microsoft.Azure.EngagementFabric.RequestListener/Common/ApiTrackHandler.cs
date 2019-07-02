// <copyright file="ApiTrackHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Common
{
    public class ApiTrackHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            var trackingId = Guid.NewGuid().ToString();

            // Log the request
            var account = RequestHelper.ParseAccount(request);
            var requestBody = await request.Content.ReadAsStringAsync();
            var requestHeader = request.Headers.ToString();

            GatewayEventSource.Current.RequestReceived(
                trackingId,
                account ?? string.Empty,
                request.RequestUri.AbsolutePath,
                request.Method.ToString(),
                requestBody ?? string.Empty,
                requestHeader);

            stopWatch.Start();
            request.Headers.Add(Constants.OperationTrackingIdHeader, trackingId);

            // Process the request
            var result = await base.SendAsync(request, cancellationToken);
            result.Headers.Add(Constants.OperationTrackingIdHeader, trackingId);

            stopWatch.Stop();

            // Log the response
            var responseBody = result.Content != null ? await result.Content.ReadAsStringAsync() : string.Empty;
            var responseHeader = result.Headers.ToString();

            GatewayEventSource.Current.ResponseSent(
                trackingId,
                result.StatusCode.ToString(),
                responseBody ?? string.Empty,
                stopWatch.ElapsedMilliseconds,
                responseHeader);

            // Audit for privileged actions
            await RequestListenerService.AuditClient.AuditIfPrivileged(request, result);

            return result;
        }
    }
}
