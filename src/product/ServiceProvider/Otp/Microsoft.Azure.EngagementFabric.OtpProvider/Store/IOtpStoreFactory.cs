// <copyright file="IOtpStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Store
{
    public interface IOtpStoreFactory
    {
        IOtpStore GetStore();
    }
}
