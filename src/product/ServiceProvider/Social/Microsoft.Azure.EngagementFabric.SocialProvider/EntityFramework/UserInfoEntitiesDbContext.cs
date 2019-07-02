// -----------------------------------------------------------------------
// <copyright file="UserInfoEntitiesDbContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.EntityFramework
{
    public partial class UserInfoEntities : DbContext
    {
        public UserInfoEntities(string connectionString)
            : base(connectionString)
        {
        }
    }
}
