// <copyright file="AdminStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.TenantCacheService.Configuration;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.Store
{
    internal class AdminStoreFactory : IAdminStoreFactory
    {
        private readonly string connectionString;

        public AdminStoreFactory(TenantConfiguration configuration)
        {
            this.connectionString = configuration.AdminStore_DefaultConnectionString;
        }

        public IAdminStore GetStore()
        {
            return new AdminStore(this.connectionString);
        }
    }
}
