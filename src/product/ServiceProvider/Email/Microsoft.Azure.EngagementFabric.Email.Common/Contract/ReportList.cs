// <copyright file="ReportList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class ReportList
    {
        [DataMember]
        public List<Report> Reports { get; set; }
    }
}
