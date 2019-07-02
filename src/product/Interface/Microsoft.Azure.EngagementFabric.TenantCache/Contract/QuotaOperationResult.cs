// <copyright file="QuotaOperationResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Net;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.TenantCache.Contract
{
    [DataContract]
    public class QuotaOperationResult
    {
        public QuotaOperationResult(HttpStatusCode status, int remaining)
        {
            this.Status = status;
            this.Remaining = remaining;
        }

        [DataMember(Name = "Status", Order = 1, IsRequired = true)]
        public HttpStatusCode Status { get; set; }

        [DataMember(Name = "Remaining", Order = 2, IsRequired = true)]
        public int Remaining { get; set; }
    }
}
