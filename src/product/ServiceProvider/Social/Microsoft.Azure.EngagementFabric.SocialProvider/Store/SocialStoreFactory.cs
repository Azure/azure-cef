// <copyright file="SocialStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Store
{
    internal class SocialStoreFactory : ISocialStoreFactory
    {
        private readonly string connectionString;
        private readonly int maxPoolSize;

        public SocialStoreFactory(string connectionString, int maxPoolSize)
        {
            this.connectionString = connectionString;
            this.maxPoolSize = maxPoolSize;
        }

        public ISocialStore GetStore()
        {
            return new SocialStore(this.connectionString, this.maxPoolSize);
        }
    }
}
