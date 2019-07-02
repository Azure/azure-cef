// <copyright file="StoreManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Store
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Configuration;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Engine;
    using Microsoft.Azure.EngagementFabric.SocialProvider.EntityFramework;
    using Microsoft.Azure.EngagementFabric.TenantCache;
    using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
    using Microsoft.WindowsAzure.Storage;

    public class StoreManager
    {
        // Telemetry storage table name
        public const string UserInfoHistoryTableName = "userinfohistory";

        private string socialServiceDbConnectionString;

        public StoreManager(string socialServiceDbConnectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(socialServiceDbConnectionString);
            var entityStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                Metadata = "res://*/EntityFramework.SocialServiceDataModel.csdl|res://*/EntityFramework.SocialServiceDataModel.ssdl|res://*/EntityFramework.SocialServiceDataModel.msl",
                ProviderConnectionString = connectionStringBuilder.ToString()
            };
            this.socialServiceDbConnectionString = entityStringBuilder.ConnectionString;
        }

        public async Task<StoreAgent> GetStoreAgent()
        {
            var targetStore = await GetTargetDb();
            var storage = await GetStorageAccount();

            return new StoreAgent(targetStore, storage);
        }

        private async Task<ISocialStore> GetTargetDb()
        {
            // TODO: allocate storage for different engagement account
            using (var ctx = new SocialEntities(socialServiceDbConnectionString))
            {
                var targetDb = await ctx.UserInfoTargetDbs.FirstOrDefaultAsync();
                Validator.IsTrue<ApplicationException>(targetDb != null, nameof(targetDb), "Failed to get target db");

                return new SocialStore(targetDb.ConnectionString, targetDb.MaxPoolSize);
            }
        }

        private async Task<CloudStorageAccount> GetStorageAccount()
        {
            // TODO: allocate storage for different engagement account
            using (var ctx = new SocialEntities(socialServiceDbConnectionString))
            {
                var storageAccount = await ctx.UserInfoStorageAccounts.FirstOrDefaultAsync();
                Validator.IsTrue<ApplicationException>(storageAccount != null, nameof(storageAccount), "Failed to get storage account");

                var cloudStorageAccount = CloudStorageAccount.Parse(storageAccount.ConnectionString);
                return cloudStorageAccount;
            }
        }
    }
}
