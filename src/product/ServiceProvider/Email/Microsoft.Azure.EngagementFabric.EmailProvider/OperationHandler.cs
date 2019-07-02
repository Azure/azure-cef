// <copyright file="OperationHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.EmailProvider.Utils;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;

namespace Microsoft.Azure.EngagementFabric.EmailProvider
{
    public sealed partial class EmailProvider
    {
        public Task<string> GetProviderName()
        {
            return Task.FromResult(Common.Constants.EmailProviderName);
        }

        /// <summary>
        /// This will be called by Gateway, when it receives tenant change event from TenantCache
        /// As all Gateway nodes are monitoring tenant change, there are multiple calls for the same event
        /// We may need to refine the logic in future, but now, simply catch any failure due to concurrent handling failure
        /// </summary>
        /// <param name="updatedTenant">Update Tenant</param>
        /// <returns>N/A</returns>
        public async Task OnTenantCreateOrUpdateAsync(Tenant updatedTenant)
        {
            if (updatedTenant == null)
            {
                return;
            }

            try
            {
                var account = await this.controller.GetAccountAsync(updatedTenant.AccountName);

                // Create
                if (account == null)
                {
                    await this.controller.CreateOrUpdateAccountAsync(new Account(updatedTenant.AccountName)
                    {
                        SubscriptionId = updatedTenant.SubscriptionId
                    });
                }

                // Update if subscriptionId changed
                else if (!string.Equals(account.SubscriptionId, updatedTenant.SubscriptionId, StringComparison.OrdinalIgnoreCase))
                {
                    account.SubscriptionId = updatedTenant.SubscriptionId;
                    await this.controller.CreateOrUpdateAccountAsync(account);
                }
            }
            catch (DbUpdateException ex)
            {
                EmailProviderEventSource.Current.Warning(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.OnTenantCreateOrUpdateAsync), OperationStates.Empty, ex.ToString());
            }
        }

        /// <summary>
        /// This will be called by Gateway, when it receives tenant delete event from TenantCache
        /// As all Gateway nodes are monitoring tenant delete, there are multiple calls for the same event
        /// We may need to refine the logic in future, but now, simply catch any failure due to concurrent handling failure
        /// </summary>
        /// <param name="tenantName">Tenant Name</param>
        /// <returns>N/A</returns>
        public async Task OnTenantDeleteAsync(string tenantName)
        {
            try
            {
                await this.controller.DeleteAccountAsync(tenantName);
            }
            catch (DbUpdateException ex)
            {
                EmailProviderEventSource.Current.Warning(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.OnTenantDeleteAsync), OperationStates.Empty, ex.ToString());
            }
        }

        public async Task<ServiceProviderResponse> OnRequestAsync(ServiceProviderRequest request)
        {
            var trackingId = request.Headers[Common.Constants.OperationTrackingIdHeader].FirstOrDefault();
            try
            {
                EmailProviderEventSource.Current.Info(trackingId, this, nameof(this.OnRequestAsync), OperationStates.Received, request.Path);

                return await this.dispatcher.DispatchAsync(
                request.HttpMethod,
                request.Path,
                request.Content,
                request.Headers,
                request.QueryNameValuePairs);
            }
            catch (Exception ex)
            {
                ExceptionHandler.ProcessException(this, ex, trackingId);
                throw;
            }
        }
    }
}
