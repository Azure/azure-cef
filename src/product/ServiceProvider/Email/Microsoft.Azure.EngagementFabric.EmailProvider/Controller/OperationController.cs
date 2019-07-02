// <copyright file="OperationController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.EmailProvider.Configuration;
using Microsoft.Azure.EngagementFabric.EmailProvider.Credential;
using Microsoft.Azure.EngagementFabric.EmailProvider.Engine;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.EmailProvider.Report;
using Microsoft.Azure.EngagementFabric.EmailProvider.Store;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Controller
{
    public sealed partial class OperationController
    {
        private IEmailStore store;
        private IEmailEngine engine;
        private ServiceConfiguration configuration;
        private Random random;
        private ServiceProxyFactory proxyFactory;
        private IReportManager reportManager;
        private ICredentialManager credentialManager;
        private MetricManager metricManager;

        public OperationController(
            IEmailStoreFactory factory,
            IEmailEngine engine,
            ServiceConfiguration configuration,
            IReportManager reportManager,
            ICredentialManager credentialManager,
            MetricManager metricManager)
        {
            this.store = factory.GetStore();
            this.engine = engine;
            this.configuration = configuration;

            this.reportManager = reportManager;
            this.credentialManager = credentialManager;
            this.metricManager = metricManager;

            this.random = new Random();
            this.proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(
                    serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });
        }

        /// <summary>
        /// For existing CEF accounts they are not initialized in db/storage
        /// Try to re-init if not exists
        /// </summary>
        /// <param name="account">CEF account name</param>
        /// <param name="trackingId">TrackingId</param>
        /// <returns>Account</returns>
        internal async Task<Account> EnsureAccount(string account, string trackingId = "")
        {
            var currentAccount = await this.store.GetAccountAsync(account);
            if (currentAccount == null)
            {
                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.EnsureAccount), OperationStates.Empty, $"Account {account} is never initialized. Try to re-init here");
                currentAccount = await this.InitAccount(account);
            }

            return currentAccount;
        }

        private async Task<Account> InitAccount(string account)
        {
            // Get subscriptionId from TenantCache
            var client = ReadOnlyTenantCacheClient.GetClient(false);
            var tenant = await client.GetTenantAsync(account);
            Validator.IsTrue<ArgumentException>(tenant != null, nameof(tenant), "Account '{0}' does not exist.", account);

            return await this.CreateOrUpdateAccountAsync(new Account(account)
            {
                SubscriptionId = tenant.SubscriptionId
            });
        }
    }
}
