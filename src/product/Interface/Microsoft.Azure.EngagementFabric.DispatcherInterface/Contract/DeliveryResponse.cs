// <copyright file="DeliveryResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    /// <summary>
    /// Delivery response returned by connector
    /// </summary>
    [DataContract]
    public class DeliveryResponse
    {
        public DeliveryResponse()
        {
        }

        public DeliveryResponse(RequestOutcome deliveryOutcome)
        {
            this.DeliveryOutcome = deliveryOutcome;
            this.ConnectorLantency = TimeSpan.Zero;
        }

        public DeliveryResponse(RequestOutcome deliveryOutcome, string detail)
            : this(deliveryOutcome)
        {
            this.DeliveryDetail = detail;
        }

        public DeliveryResponse(RequestOutcome deliveryOutcome, string detail, string customMessageId)
            : this(deliveryOutcome, detail)
        {
            this.CustomMessageId = customMessageId;
        }

        [DataMember]
        public RequestOutcome DeliveryOutcome { get; set; }

        [DataMember]
        public TimeSpan ConnectorLantency { get; set; }

        [DataMember]
        public string DeliveryDetail { get; set; }

        [DataMember]
        public string CustomMessageId { get; set; }

        public override string ToString()
        {
            return $"outcome={this.DeliveryOutcome} latency={ConnectorLantency} detail={this.DeliveryDetail} customMessageId={this.CustomMessageId} ";
        }
    }
}
