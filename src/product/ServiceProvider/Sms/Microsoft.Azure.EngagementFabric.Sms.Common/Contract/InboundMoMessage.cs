// <copyright file="InboundMoMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public class InboundMoMessage
    {
        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public DateTime? InboundTime { get; set; }

        [DataMember]
        public string Content { get; set; }

        [DataMember]
        public string ExtendedCode { get; set; }
    }
}
