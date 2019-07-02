// -----------------------------------------------------------------------
// <copyright file="SmsServiceDbEntitiesDbContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Entity;
using Microsoft.Azure.EngagementFabric.Common.Db;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.EntityFramework
{
    [DbConfigurationType(typeof(DataContextConfiguration))]
    public partial class SmsServiceDbEntities : DbContext
    {
        public SmsServiceDbEntities(string connectionString)
            : base(connectionString)
        {
        }
    }
}
