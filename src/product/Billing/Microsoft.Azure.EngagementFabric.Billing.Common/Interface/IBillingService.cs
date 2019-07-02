// <copyright file="IBillingService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Microsoft.Azure.EngagementFabric.Billing.Common.Interface
{
    public interface IBillingService : IService
    {
        Task ReportBillingUsageAsync(List<ResourceUsageRecord> records, CancellationToken cancellationToken);
    }
}
