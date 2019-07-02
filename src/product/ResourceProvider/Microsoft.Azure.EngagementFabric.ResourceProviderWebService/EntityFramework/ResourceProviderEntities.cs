// <copyright file="ResourceProviderEntities.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Data.Entity;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.EntityFramework
{
    /// <summary>
    /// Partial implementation of ResourceProviderEntities to introduce constructor takes connection string as parameter
    /// </summary>
    internal partial class ResourceProviderEntities : DbContext
    {
        public ResourceProviderEntities(string connectionString)
            : base(connectionString)
        {
            this.SubscriptionRegistrations = this.Set<SubscriptionRegistration>();
        }
    }
}
