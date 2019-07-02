// -----------------------------------------------------------------------
// <copyright file="TenantConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Fabric;
using Microsoft.Azure.EngagementFabric.Common.Extension;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService.Configuration
{
    public class TenantConfiguration
    {
        private readonly ICodePackageActivationContext context;

        public TenantConfiguration(ICodePackageActivationContext context)
        {
            this.context = context;
            this.AdminStore_DefaultConnectionString = this.context.GetConfig<string>("AdminStore", "DefaultConnectionString");
            this.TenantCache_DefaultConnectionString = this.context.GetConfig<string>("TenantCache", "DefaultConnectionString");
            this.TenantCache_DatabaseId = this.context.GetConfig<int>("TenantCache", "DatabaseId");
            this.TenantCache_QuotaDatabaseId = this.context.GetConfig<int>("TenantCache", "QuotaDatabaseId");
        }

        public string AdminStore_DefaultConnectionString { get; set; }

        public string TenantCache_DefaultConnectionString { get; set; }

        public int TenantCache_DatabaseId { get; set; }

        public int TenantCache_QuotaDatabaseId { get; set; }
    }
}