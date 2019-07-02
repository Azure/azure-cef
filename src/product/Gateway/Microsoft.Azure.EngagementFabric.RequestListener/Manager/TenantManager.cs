// <copyright file="TenantManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.TenantCache;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Manager
{
    public class TenantManager : IDisposable
    {
        private ReadOnlyTenantCacheClient tenantCacheClient = ReadOnlyTenantCacheClient.GetClient(true);

        public TenantManager()
        {
            this.tenantCacheClient.TenantChangedEventHandler += TenantCacheClient_TenantChangedEventHandler;
        }

        public void Dispose()
        {
            this.tenantCacheClient.TenantChangedEventHandler -= TenantCacheClient_TenantChangedEventHandler;
        }

        private void TenantCacheClient_TenantChangedEventHandler(object sender, TenantCache.Contract.TenantChangedEventArgs e)
        {
            GatewayEventSource.Current.Info(GatewayEventSource.EmptyTrackingId, this, nameof(this.TenantCacheClient_TenantChangedEventHandler), OperationStates.Received, $"Receive tenant event. EventType={e.EventType} TenantName={e.TenantName}");

            try
            {
                var providers = ProviderManager.GetAllProviders();
                var tasks = new List<Task>();
                switch (e.EventType)
                {
                    case TenantCache.Contract.TenantChangedEventType.TenantUpdated:

                        foreach (var provider in providers)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await provider.OnTenantCreateOrUpdateAsync(e.UpdatedTenant);
                                }
                                catch (Exception ex)
                                {
                                    GatewayEventSource.Current.ErrorException(GatewayEventSource.EmptyTrackingId, this, "provider.OnTenantCreateOrUpdateAsync", OperationStates.Failed, string.Empty, ex);
                                }
                            }));
                        }

                        break;
                    case TenantCache.Contract.TenantChangedEventType.TenantDeleted:

                        foreach (var provider in providers)
                        {
                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await provider.OnTenantDeleteAsync(e.TenantName);
                                }
                                catch (Exception ex)
                                {
                                    GatewayEventSource.Current.ErrorException(GatewayEventSource.EmptyTrackingId, this, "provider.OnTenantDeleteAsync", OperationStates.Failed, string.Empty, ex);
                                }
                            }));
                        }

                        break;
                }

                Task.WhenAll(tasks).Wait();
            }
            catch (Exception ex)
            {
                GatewayEventSource.Current.ErrorException(GatewayEventSource.EmptyTrackingId, this, nameof(this.TenantCacheClient_TenantChangedEventHandler), OperationStates.Failed, string.Empty, ex);
            }
        }
    }
}
