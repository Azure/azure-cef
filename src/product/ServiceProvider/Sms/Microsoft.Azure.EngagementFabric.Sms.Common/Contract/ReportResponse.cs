// <copyright file="ReportResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public class ReportResponse
    {
        public ReportResponse(RequestOutcome outcome, List<ReportDetail> reports)
        {
            this.RequestOutcome = outcome;
            this.Details = reports;
        }

        [DataMember]
        public List<ReportDetail> Details { get; set; }

        [DataMember]
        public RequestOutcome RequestOutcome { get; set; }
    }
}
