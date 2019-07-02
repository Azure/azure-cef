// <copyright file="AdminStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.RequestListener.Store
{
    internal class AdminStoreFactory : IAdminStoreFactory
    {
        public IAdminStore GetStore()
        {
            return new AdminStore();
        }
    }
}
