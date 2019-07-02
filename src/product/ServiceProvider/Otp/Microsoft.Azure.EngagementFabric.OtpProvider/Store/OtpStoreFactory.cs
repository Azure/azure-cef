// <copyright file="OtpStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Store
{
    internal class OtpStoreFactory : IOtpStoreFactory
    {
        private readonly string connectionString;
        private readonly int maxPoolSize;

        public OtpStoreFactory(string connectionString, int maxPoolSize)
        {
            this.connectionString = connectionString;
            this.maxPoolSize = maxPoolSize;
        }

        public IOtpStore GetStore()
        {
            return new OtpStore(this.connectionString, this.maxPoolSize);
        }
    }
}
