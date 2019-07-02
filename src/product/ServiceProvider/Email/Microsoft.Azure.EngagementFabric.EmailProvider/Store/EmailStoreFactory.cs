// <copyright file="EmailStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Store
{
    public class EmailStoreFactory : IEmailStoreFactory
    {
        private readonly string connectionString;

        public EmailStoreFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public IEmailStore GetStore()
        {
            return new EmailStore(this.connectionString);
        }
    }
}
