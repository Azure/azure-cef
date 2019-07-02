// <copyright file="DataContextConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Microsoft.Azure.EngagementFabric.Common.Db
{
    public class DataContextConfiguration : DbConfiguration
    {
        public DataContextConfiguration()
        {
            SetExecutionStrategy("System.Data.SqlClient", () => new SqlAzureExecutionStrategy());
        }
    }
}
