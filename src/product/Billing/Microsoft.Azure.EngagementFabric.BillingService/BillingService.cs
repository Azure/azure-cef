// <copyright file="BillingService.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;
using Microsoft.Azure.EngagementFabric.Billing.Common.Interface;
using Microsoft.Azure.EngagementFabric.BillingService.Configuration;
using Microsoft.Azure.EngagementFabric.BillingService.Manager;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Azure.EngagementFabric.BillingService
{
    /// <summary>
    /// An instance of this class is created for each service replica by the Service Fabric runtime.
    /// </summary>
    internal sealed class BillingService : StatefulService, IBillingService
    {
        private ServiceConfiguration configuration;
        private BillingManager billingManager;

        public BillingService(StatefulServiceContext context)
            : base(context)
        {
        }

        public async Task ReportBillingUsageAsync(List<ResourceUsageRecord> records, CancellationToken cancellationToken)
        {
            if (records == null || records.Count <= 0)
            {
                return;
            }

            await this.billingManager.StoreBillingUsageAsync(records, cancellationToken);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener((c) =>
                {
                    return new FabricTransportServiceRemotingListener(c, this, null, new ServiceRemotingJsonSerializationProvider());
                })
            };
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            this.configuration = new ServiceConfiguration(Context.CodePackageActivationContext);
            this.billingManager = new BillingManager(configuration, StateManager);
            await this.billingManager.OnRunAsync(cancellationToken);
        }
    }
}
