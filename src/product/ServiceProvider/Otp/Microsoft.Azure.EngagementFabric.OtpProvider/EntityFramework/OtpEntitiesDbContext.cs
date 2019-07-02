// -----------------------------------------------------------------------
// <copyright file="OtpEntitiesDbContext.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Data.Entity;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.EntityFramework
{
    public partial class OtpEntities : DbContext
    {
        public OtpEntities(string connectionString)
            : base(connectionString)
        {
        }
    }
}
