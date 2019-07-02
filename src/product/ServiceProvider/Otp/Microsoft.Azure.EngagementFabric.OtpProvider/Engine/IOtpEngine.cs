// <copyright file="IOtpEngine.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Engine
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Contract;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;

    public interface IOtpEngine
    {
        Task<ServiceProviderResponse> OtpPushAsync(string account, OtpPushDescription description, ServiceProviderRequest request, string requestId, CancellationToken cancellationToken);

        Task<OtpCheckOperationResult> OtpCheckAsync(string account, OtpCheckDescription description, string requestId, CancellationToken cancellationToken);

        Task CreateOtpAccountAsync(string account);

        Task DeleteOtpAccountAsync(string account);
    }
}
