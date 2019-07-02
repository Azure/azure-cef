// <copyright file="IReportingService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface
{
    public interface IReportingService : IService
    {
        Task ReportDispatcherResultsAsync(ReadOnlyCollection<OutputResult> results);
    }
}
