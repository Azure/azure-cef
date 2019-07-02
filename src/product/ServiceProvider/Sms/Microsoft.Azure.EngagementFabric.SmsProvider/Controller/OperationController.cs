// <copyright file="OperationController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.SmsProvider.Configuration;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Microsoft.Azure.EngagementFabric.SmsProvider.Inbound;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Report;
using Microsoft.Azure.EngagementFabric.SmsProvider.Store;
using Microsoft.Azure.EngagementFabric.SmsProvider.Utils;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Controller
{
    public sealed partial class OperationController
    {
        private ISmsStore store;
        private ServiceConfiguration configuration;
        private Random random;
        private ServiceProxyFactory proxyFactory;
        private IReportManager reportManager;
        private IInboundManager inboundManager;
        private ICredentialManager credentialManager;
        private MetricManager metricManager;
        private MailHelper mailHelper;

        public OperationController(
            ISmsStoreFactory factory,
            ServiceConfiguration configuration,
            IReportManager reportManager,
            IInboundManager inboundManager,
            ICredentialManager credentialManager,
            MetricManager metricManager)
        {
            this.store = factory.GetStore();
            this.configuration = configuration;
            this.reportManager = reportManager;
            this.inboundManager = inboundManager;
            this.credentialManager = credentialManager;
            this.metricManager = metricManager;

            this.random = new Random();
            this.proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(
                    serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });

            if (this.configuration.SmsOpsInfo.IsValid())
            {
                this.mailHelper = new MailHelper(this.configuration.SmsOpsInfo);
            }
        }

        private async Task<Account> ValidateAccount(string account)
        {
            var currentAccount = await this.store.GetAccountAsync(account);
            if (currentAccount == null)
            {
                SmsProviderEventSource.Current.Critical(SmsProviderEventSource.EmptyTrackingId, this, nameof(this.ValidateAccount), OperationStates.Empty, $"Account {account} is never initialized. Try to re-init here");

                // Get subscriptionId
                var client = ReadOnlyTenantCacheClient.GetClient(false);
                var tenant = await client.GetTenantAsync(account);
                currentAccount = await this.CreateOrUpdateAccountAsync(new Account(account)
                {
                    SubscriptionId = tenant.SubscriptionId
                });
            }

            Validator.IsTrue<ArgumentException>(currentAccount != null, nameof(currentAccount), "Account '{0}' does not exist.", account);
            return currentAccount;
        }
    }
}
