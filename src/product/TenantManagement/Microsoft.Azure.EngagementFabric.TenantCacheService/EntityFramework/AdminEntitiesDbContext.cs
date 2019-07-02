// -----------------------------------------------------------------------
// <copyright file="AdminEntitiesDbContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Entity;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.EntityFramework
{
    public partial class AdminEntities : DbContext
    {
        public AdminEntities(string connectionString)
            : base(connectionString)
        {
        }
    }
}
