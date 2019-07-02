// <copyright file="InboundMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public class InboundMessage
    {
        public InboundMessage()
        {
        }

        [DataMember]
        public InboundMoMessage MoMessage { get; set; }

        [DataMember]
        public ReportDetail ReportMessage { get; set; }
    }
}
