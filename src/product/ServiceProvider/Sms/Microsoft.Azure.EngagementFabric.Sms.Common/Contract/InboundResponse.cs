// <copyright file="InboundResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public class InboundResponse
    {
        public InboundResponse()
        {
        }

        public InboundResponse(InboundType type)
        {
            this.Type = type;
        }

        [DataMember]
        public InboundType Type { get; set; }

        [DataMember]
        public InboundHttpResponseMessage Response { get; set; }

        [DataMember]
        public Dictionary<ConnectorIdentifier, List<InboundMessage>> Messages { get; set; }
    }
}
