// <copyright file="BillingAgent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Billing.Common;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;
using Microsoft.Azure.EngagementFabric.Billing.Common.Interface;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Communication.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Billing
{
    public class BillingAgent : BillingStore
    {
        private static readonly Uri BillingServiceUri = new Uri("fabric:/BillingApp/BillingService");
        private ServiceProxyFactory proxyFactory;
        private ConcurrentBag<ResourceUsageRecord> bucket;
        private ReaderWriterLockSlim bucketStateLock;

        public BillingAgent()
            : base(TimeSpan.FromMinutes(1), TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(1), 100, 10)
        {
            this.proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(
                    serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });

            this.bucket = new ConcurrentBag<ResourceUsageRecord>();
            this.bucketStateLock = new ReaderWriterLockSlim();
        }

        public override Task OnRunAsync(CancellationToken cancellationToken)
        {
            return base.OnRunAsync(cancellationToken);
        }

        public override async Task OnCloseAsync()
        {
            if (bucket.Count > 0)
            {
                await PushToTargetAsync(bucket.ToList(), Guid.NewGuid(), CancellationToken.None);
                bucket = new ConcurrentBag<ResourceUsageRecord>();
            }

            await base.OnCloseAsync();
        }

        public override Task StoreBillingUsageAsync(IEnumerable<ResourceUsageRecord> records, CancellationToken cancellationToken)
        {
            if (records == null || records.Count() <= 0)
            {
                return Task.CompletedTask;
            }

            this.bucketStateLock.EnterReadLock();

            try
            {
                foreach (var record in records)
                {
                    this.bucket.Add(record);
                }
            }
            finally
            {
                this.bucketStateLock.ExitReadLock();
            }

            return Task.CompletedTask;
        }

        protected override async Task PushUsageInnerLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (bucket.Count > 0)
                {
                    ConcurrentBag<ResourceUsageRecord> closedBucket = null;
                    this.bucketStateLock.EnterWriteLock();
                    try
                    {
                        closedBucket = bucket;
                        bucket = new ConcurrentBag<ResourceUsageRecord>();
                    }
                    finally
                    {
                        this.bucketStateLock.ExitWriteLock();
                    }

                    // Wait for 10s to ensure all in-flight requests are done
                    await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
                    await PushToTargetAsync(closedBucket.ToList(), Guid.NewGuid(), CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PushUsageInnerLoopAsync), OperationStates.Failed, "PushUsageInnerLoopAsync failed with exception", ex);
            }
        }

        protected override async Task PushToTargetAsync(IEnumerable<ResourceUsageRecord> records, Guid batchId, CancellationToken cancellationToken)
        {
            // PartionKey is between Int.Min and Int.Max
            var partitionKey = new ServicePartitionKey(batchId.GetHashCode());
            var client = this.proxyFactory.CreateServiceProxy<IBillingService>(BillingServiceUri, partitionKey, TargetReplicaSelector.PrimaryReplica);
            await client.ReportBillingUsageAsync(records.ToList(), cancellationToken);

            EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PushUsageInnerLoopAsync), OperationStates.Succeeded, $"Pushed {records.Count()} record(s) to Billing Service");
        }
    }
}
