// <copyright file="OperationHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Azure.EngagementFabric.SmsProvider.Utils;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;

namespace Microsoft.Azure.EngagementFabric.SmsProvider
{
    public sealed partial class SmsProvider
    {
        public Task<string> GetProviderName()
        {
            return Task.FromResult(Common.Constants.SmsProviderName);
        }

        public async Task OnTenantCreateOrUpdateAsync(Tenant updatedTenant)
        {
            if (updatedTenant == null)
            {
                return;
            }

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

        public async Task OnTenantDeleteAsync(string tenantName)
        {
            await this.controller.DeleteAccountAsync(tenantName);
        }

        public async Task<ServiceProviderResponse> OnRequestAsync(ServiceProviderRequest request)
        {
            var trackingId = request.Headers[Common.Constants.OperationTrackingIdHeader].FirstOrDefault();
            try
            {
                SmsProviderEventSource.Current.Info(trackingId, this, nameof(this.OnRequestAsync), OperationStates.Received, request.Path);

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
