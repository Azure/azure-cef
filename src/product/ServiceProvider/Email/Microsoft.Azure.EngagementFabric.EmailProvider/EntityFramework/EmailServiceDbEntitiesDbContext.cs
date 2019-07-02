// -----------------------------------------------------------------------
// <copyright file="EmailServiceDbEntitiesDbContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Entity;
using Microsoft.Azure.EngagementFabric.Common.Db;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.EntityFramework
{
    [DbConfigurationType(typeof(DataContextConfiguration))]
    public partial class EmailServiceDbEntities : DbContext
    {
        public EmailServiceDbEntities(string connectionString)
            : base(connectionString)
        {
        }
    }
}
