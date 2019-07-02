// <copyright file="DeliveryRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    /// <summary>
    /// Dispatcher object sent to connector
    /// </summary>
    [DataContract]
    public class DeliveryRequest
    {
        public DeliveryRequest(OutputMessage outputMessae, DateTime requestExpiration)
        {
            this.OutputMessage = outputMessae;
            this.OutputMessage.RequestExpiration = requestExpiration;
            this.DeliverRequestTimestamp = DateTime.UtcNow;
        }

        [DataMember]
        public OutputMessage OutputMessage { get; set; }

        [DataMember]
        public DateTime DeliverRequestTimestamp { get; set; }

        public void Succeed()
        {
            this.OutputMessage.Delivered = true;
        }

        public void Failed()
        {
            this.OutputMessage.Delivered = false;
            this.OutputMessage.RequestExpiration = DateTime.MinValue;
        }
    }
}
