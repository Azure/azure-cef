// -----------------------------------------------------------------------
// <copyright file="SocialEntitiesDbContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Entity;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.EntityFramework
{
    public partial class SocialEntities : DbContext
    {
        public SocialEntities(string connectionString)
            : base(connectionString)
        {
        }
    }
}
