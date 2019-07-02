// -----------------------------------------------------------------------
// <copyright file="IOperationHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.ServiceFabric.Services.Remoting;

namespace Microsoft.Azure.EngagementFabric.ProviderInterface
{
    /// <summary>
    /// Service provider interface for operation management
    /// </summary>
    public partial interface IServiceProvider : IService
    {
        Task<string> GetProviderName();

        Task OnTenantCreateOrUpdateAsync(Tenant updatedTenant);

        Task OnTenantDeleteAsync(string tenantName);

        Task<ServiceProviderResponse> OnRequestAsync(ServiceProviderRequest request);
    }
}
