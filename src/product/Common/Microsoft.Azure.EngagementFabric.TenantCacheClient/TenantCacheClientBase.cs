// <copyright file="TenantCacheClientBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Concurrent;
using Microsoft.Azure.EngagementFabric.Common.Cache;
using Microsoft.Azure.EngagementFabric.Common.Serialization;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Microsoft.Azure.EngagementFabric.TenantCache
{
    // It uses redis cache, and is optional to use in memory cache to reduce request latency.
    // For create or update scenario, send request to TenantCacheService and listen to event of cache changed.
    // For get scenario, read from 1) in memory cache 2) redis 3) TenantCacheService.
    public class TenantCacheClientBase
    {
        private const string CacheServiceUri = "fabric:/TenantManagementApp/TenantCacheService";

        protected TenantCacheClientBase(bool enableInMemoryCache)
        {
            this.EnableInMemoryCache = enableInMemoryCache;
            if (this.EnableInMemoryCache)
            {
                this.MemoryCache = new ConcurrentDictionary<string, Tenant>(StringComparer.OrdinalIgnoreCase);
            }

            // Connect with TenantCacheService
            var proxyFactory = new ServiceProxyFactory((c) =>
            {
                return new FabricTransportServiceRemotingClientFactory(serializationProvider: new ServiceRemotingJsonSerializationProvider());
            });
            this.CacheProxy = proxyFactory.CreateServiceProxy<ITenantCache>(new Uri(CacheServiceUri));

            // Initialize RedisClient
            var config = this.CacheProxy.GetCacheConfiguration().Result;
            this.RedisClient = new RedisClient(config.ConnectionString, config.DatabaseId);
            this.RedisClient.SubscribeAsync(typeof(TenantChangedEventArgs).Name, (channel, value) => this.OnTenantChanged(value)).Wait();
        }

        // This is optional in case caller service will perform any action on tenant changed.
        // Cache update will be handled by this client itself.
        public event EventHandler<TenantChangedEventArgs> TenantChangedEventHandler;

        /// <summary>
        /// Gets the TenantCache service
        /// </summary>
        protected ITenantCache CacheProxy { get; }

        /// <summary>
        /// Gets a value indicating whether enable the In-memory cache
        /// </summary>
        protected bool EnableInMemoryCache { get; }

        /// <summary>
        /// Gets the cache of tenant
        /// </summary>
        protected ConcurrentDictionary<string, Tenant> MemoryCache { get; }

        /// <summary>
        /// Gets the redis cache client
        /// </summary>
        protected RedisClient RedisClient { get; }

        private void OnTenantChanged(RedisValue value)
        {
            if (!value.HasValue || value.IsNullOrEmpty)
            {
                return;
            }

            var eventArgs = JsonConvert.DeserializeObject<TenantChangedEventArgs>(value.ToString());
            if (eventArgs == null)
            {
                return;
            }

            // Update in memory cache
            if (this.EnableInMemoryCache)
            {
                switch (eventArgs.EventType)
                {
                    case TenantChangedEventType.TenantUpdated:
                        this.MemoryCache.AddOrUpdate(eventArgs.TenantName, eventArgs.UpdatedTenant, (key, old) => eventArgs.UpdatedTenant);
                        break;

                    case TenantChangedEventType.TenantDeleted:
                        Tenant removed;
                        this.MemoryCache.TryRemove(eventArgs.TenantName, out removed);
                        break;
                }
            }

            // Fire event
            this.TenantChangedEventHandler?.Invoke(this, eventArgs);
        }
    }
}