// <copyright file="SubscriptionManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.EntityFramework;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Store;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.TenantCache;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers
{
    internal class SubscriptionManager : ISubscriptionManager
    {
        private readonly IResourceProviderStore store;
        private readonly FullTenantCacheClient tenantCacheClient;

        public SubscriptionManager(IResourceProviderStore store)
        {
            this.store = store;
            this.tenantCacheClient = FullTenantCacheClient.GetClient(true);
        }

        public async Task<IEnumerable<string>> CreateOrUpdateSubscriptionAsync(
            string requestId,
            string subscriptionId,
            SubscriptionDescription model)
        {
            // The racing condition will be handled by two-steps updating

            // Step 1: Update the subscription registration
            var subscriptionRegistration = new SubscriptionRegistration
            {
                SubscriptionId = subscriptionId,
                State = model.State.ToString(),
                TenantId = model.Properties.TenantId,
                RegistrationDate = model.RegistrationDate,
                LocationPlacementId = model.Properties.LocationPlacementId ?? "n/a",
                QuotaId = model.Properties.QuotaId ?? "n/a",
                RegisteredFeatures = JsonConvert.SerializeObject(model.Properties.RegisteredFeatures) ?? "n/a"
            };

            await this.store.SetSubscriptionRegistrationAsync(
                subscriptionId,
                subscriptionRegistration);

            // Step 2: Update the per-account state
            var tenants = await this.tenantCacheClient.ListTenantsAsync(
                requestId,
                subscriptionId);

            switch (model.State)
            {
                case SubscriptionState.Registered:
                    {
                        foreach (var tenant in tenants)
                        {
                            tenant.IsDisabled = false;

                            try
                            {
                                await this.tenantCacheClient.UpdateTenantAsync(
                                    requestId,
                                    tenant);
                            }
                            catch
                            {
                                // The account may be removed by some other concurrent action
                            }
                        }
                    }

                    break;

                case SubscriptionState.Warned:
                case SubscriptionState.Suspended:
                    {
                        foreach (var tenant in tenants)
                        {
                            tenant.IsDisabled = true;

                            try
                            {
                                await this.tenantCacheClient.UpdateTenantAsync(
                                    requestId,
                                    tenant);
                            }
                            catch
                            {
                                // The account may be removed by some other concurrent action
                            }
                        }
                    }

                    break;

                case SubscriptionState.Deleted:
                case SubscriptionState.Unregistered:
                    {
                        foreach (var tenant in tenants)
                        {
                            try
                            {
                                await this.tenantCacheClient.DeleteTenantAsync(
                                    requestId,
                                    subscriptionId,
                                    tenant.ResourceGroupName,
                                    tenant.AccountName);
                            }
                            catch
                            {
                                // The account may be removed by some other concurrent action
                            }
                        }
                    }

                    break;

                default:
                    break;
            }

            return tenants.Select(t => t.AccountName);
        }
    }
}
