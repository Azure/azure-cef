// <copyright file="IOtpStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Store
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Contract;
    using Microsoft.Azure.EngagementFabric.OtpProvider.EntityFramework;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Telemetry;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;

    public interface IOtpStore
    {
        Task<OtpCode> CreateorUpdateOtpCodeAsync(string engagementAccount, string phoneNumber, string code, int expireTime);

        Task DeleteOtpCodeAsync(string engagementAccount, string phoneNumber);

        Task<OtpCode> QueryOtpCodeAsync(string engagementAccount, string phoneNumber);

        Task DeleteOtpCodeByTimeAsync(DateTime expiredTime);

        Task DeleteOtpAccountDataAsync(string account);
    }
}
