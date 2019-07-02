// <copyright file="SmsStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Store
{
    public class SmsStoreFactory : ISmsStoreFactory
    {
        private readonly string connectionString;

        public SmsStoreFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public ISmsStore GetStore()
        {
            return new SmsStore(this.connectionString);
        }
    }
}
