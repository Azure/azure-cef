// <copyright file="IAdminStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.Store
{
    internal interface IAdminStoreFactory
    {
        IAdminStore GetStore();
    }
}
