// <copyright file="ReportDetail.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public class ReportDetail
    {
        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public MessageState State { get; set; }

        [DataMember]
        public DateTime? ReceiveTime { get; set; }

        [DataMember]
        public DateTime? SubmitTime { get; set; }

        [DataMember]
        public string StateDetail { get; set; }

        [DataMember]
        public string CustomMessageId { get; set; }
    }
}
