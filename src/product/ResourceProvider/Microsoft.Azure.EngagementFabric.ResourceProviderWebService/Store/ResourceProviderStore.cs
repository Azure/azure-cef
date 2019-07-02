// <copyright file="ResourceProviderStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.EntityFramework;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Store
{
    internal class ResourceProviderStore : IResourceProviderStore
    {
        private string connectionString;

        public ResourceProviderStore(string connectionString)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            var entityStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                Metadata = "res://*/EntityFramework.ResourceProviderStoreDataModel.csdl|res://*/EntityFramework.ResourceProviderStoreDataModel.ssdl|res://*/EntityFramework.ResourceProviderStoreDataModel.msl",
                ProviderConnectionString = connectionStringBuilder.ToString()
            };

            this.connectionString = entityStringBuilder.ConnectionString;
        }

        public async Task<SubscriptionRegistration> GetSubscriptionRegistrationAsync(
            string subscriptionId)
        {
            using (var ctx = new ResourceProviderEntities(this.connectionString))
            {
                return await ctx.SubscriptionRegistrations.FindAsync(subscriptionId);
            }
        }

        public async Task<bool> IsSubscriptionRegisteredAsync(
            string subscriptionId)
        {
            var subscriptionRegistration = await this.GetSubscriptionRegistrationAsync(subscriptionId);
            return string.Equals(subscriptionRegistration?.State, SubscriptionState.Registered.ToString());
        }

        public async Task SetSubscriptionRegistrationAsync(
            string subscriptionId,
            SubscriptionRegistration subscriptionRegistration)
        {
            using (var ctx = new ResourceProviderEntities(this.connectionString))
            {
                ctx.SubscriptionRegistrations.AddOrUpdate(subscriptionRegistration);
                await ctx.SaveChangesAsync();
            }
        }
    }
}
