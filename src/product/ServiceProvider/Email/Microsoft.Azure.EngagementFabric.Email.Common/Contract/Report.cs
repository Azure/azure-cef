// <copyright file="Report.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class Report
    {
        [DataMember]
        public MessageIdentifer MessageIdentifer { get; set; }

        [DataMember]
        public int TotalTarget { get; set; }

        [DataMember]
        public int TotalDelivered { get; set; }

        [DataMember]
        public int TotalOpened { get; set; }

        [DataMember]
        public int TotalClicked { get; set; }
    }
}
