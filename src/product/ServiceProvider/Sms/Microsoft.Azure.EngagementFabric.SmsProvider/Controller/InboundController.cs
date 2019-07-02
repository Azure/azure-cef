// <copyright file="InboundController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.SmsProvider.Inbound;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Controller
{
    public class InboundController : ApiController
    {
        private IInboundManager inboundManager;

        public IInboundManager InboundManager
        {
            get
            {
                if (this.inboundManager == null)
                {
                    this.inboundManager = (IInboundManager)Configuration.Properties[typeof(IInboundManager).Name];
                }

                return this.inboundManager;
            }
        }

        [Route("sms/callback/inbound/{connectorName}")]
        [HttpPost]
        public async Task<HttpResponseMessage> OnInboundMessageReceived(string connectorName)
        {
            SmsProviderEventSource.Current.Info(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.OnInboundMessageReceived), OperationStates.Received, $"Inbound message received from {connectorName}");
            return await InboundManager.OnInboundMessageReceived(connectorName, Request, CancellationToken.None);
        }

        [Route("sms/callback/inbound/{connectorName}/{connectorId}")]
        [HttpPost]
        public async Task<HttpResponseMessage> OnInboundMessageReceivedWithConnectorId(string connectorName, string connectorId)
        {
            var trackingId = Guid.NewGuid().ToString();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            SmsProviderEventSource.Current.Info(trackingId, this, nameof(this.OnInboundMessageReceivedWithConnectorId), OperationStates.Received, $"Inbound message received from name={connectorName} id={connectorId} content={await Request.Content.ReadAsStringAsync()}");

            var response = await InboundManager.OnInboundMessageReceived(connectorName, Request, CancellationToken.None);
            stopwatch.Stop();
            SmsProviderEventSource.Current.Info(trackingId, this, nameof(this.OnInboundMessageReceivedWithConnectorId), OperationStates.Succeeded, $"Inbound message received from name={connectorName} id={connectorId} elapse={stopwatch.Elapsed} code={response.StatusCode}");

            return response;
        }
    }
}
