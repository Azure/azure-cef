// <copyright file="DomainController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;
using EmailConstant = Microsoft.Azure.EngagementFabric.EmailProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Controller
{
    public sealed partial class OperationController
    {
        [HttpPost]
        [Route("admin/accounts/{account}/domains")]
        public async Task<ServiceProviderResponse> CreateOrUpdateDomainAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account,
            [FromBody] Domain request)
        {
            Validator.ArgumentNotNull(request, nameof(request));
            ValidateDomain(request.Value);

            var currentAccount = await EnsureAccount(account, requestId);

            var oldState = ResourceState.Unknown;
            var domains = await this.store.GetDomainsByNameAsync(request.Value);
            if (domains != null)
            {
                // Check if domain is used by other
                Validator.IsTrue<ArgumentException>(
                    !domains.Any(d => d.State == ResourceState.Active && !d.EngagementAccount.Equals(currentAccount.EngagementAccount, StringComparison.OrdinalIgnoreCase)),
                    nameof(domains),
                    "Domain '{0}' is alreay used by other account.",
                    request.Value);
            }

            var domain = domains?.SingleOrDefault(d => d.EngagementAccount.Equals(currentAccount.EngagementAccount, StringComparison.OrdinalIgnoreCase));
            if (domain == null)
            {
                // Add restriction that do not create an active Domain directly
                // Create a Domain with pending status first
                // This is to prevent admin providing incorrect name when activate a Domain
                Validator.IsTrue<ArgumentException>(request.State != ResourceState.Active, nameof(request.State), "Cannot create a domain with active state.");

                domain = new Domain();
                domain.EngagementAccount = currentAccount.EngagementAccount;
                domain.Value = request.Value;
            }
            else
            {
                oldState = domain.State;
            }

            domain.State = request.State;
            domain.Message = request.Message;
            domain = await this.store.CreateOrUpdateDomainAsync(domain);

            // Disable all active templates if Domain is not in active state, and vice versa
            if (oldState == ResourceState.Active && domain.State != ResourceState.Active)
            {
                await this.engine.UpdateTemplateStateByDomainAsync(currentAccount.EngagementAccount, domain.Value, ResourceState.Active, ResourceState.Disabled, $"Disabled because domain is in state '{domain.State.ToString()}'");
            }
            else if (oldState != ResourceState.Active && domain.State == ResourceState.Active)
            {
                try
                {
                    // Ensure EmailAccount exist when setting an active domain
                    await this.credentialManager.EnsureEmailAccountAsync(currentAccount.EngagementAccount);
                }
                catch (Exception ex)
                {
                    EmailProviderEventSource.Current.CriticalException(requestId, this, nameof(this.CreateOrUpdateDomainAsync), OperationStates.Failed, $"Failed to create EmailAccount for account {account}", ex);

                    // Reset domain state to oldState
                    domain.State = oldState;
                    domain.Message = null;
                    domain = await this.store.CreateOrUpdateDomainAsync(domain);

                    throw;
                }

                await this.engine.UpdateTemplateStateByDomainAsync(currentAccount.EngagementAccount, domain.Value, ResourceState.Disabled, ResourceState.Active, null);
            }

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, domain);
        }

        [HttpGet]
        [Route("domains")]
        public async Task<ServiceProviderResponse> ListDomainsAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            [FromQuery] string continuationToken = null,
            [FromQuery] int count = ContinuationToken.DefaultCount)
        {
            var currentAccount = await EnsureAccount(account, requestId);
            Validator.IsTrue<ArgumentException>(count > 0 && count <= EmailConstant.PagingMaxTakeCount, nameof(count), $"Count should be between 0 and {EmailConstant.PagingMaxTakeCount}.");

            var token = new DbContinuationToken(continuationToken);
            Validator.IsTrue<ArgumentException>(token.IsValid, nameof(token), "ContinuationToken is invalid.");

            var domains = await this.store.ListDomainsByAccountAsync(currentAccount.EngagementAccount, token, count);
            var response = ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, domains);
            if (domains.NextLink != null)
            {
                response.Headers = new Dictionary<string, IEnumerable<string>>
                {
                    { ContinuationToken.ContinuationTokenKey, new List<string> { domains.NextLink.Token } }
                };
            }

            return response;
        }

        [HttpGet]
        [Route("domains/{value}")]
        public async Task<ServiceProviderResponse> GetDomainAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            var domain = await this.store.GetDomainAsync(currentAccount.EngagementAccount, value);
            Validator.IsTrue<ResourceNotFoundException>(domain != null, nameof(domain), "The Domain '{0}' does not exist.", value);

            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, domain);
        }

        [HttpDelete]
        [Route("domains/{value}")]
        public async Task<ServiceProviderResponse> DeleteDomainAsync(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromHeader] string account,
            string value)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            var domain = await this.store.GetDomainAsync(currentAccount.EngagementAccount, value);
            Validator.IsTrue<ResourceNotFoundException>(domain != null, nameof(domain), "The Domain '{0}' does not exist.", value);

            await this.engine.DeleteSendersbyDomainAsync(currentAccount.EngagementAccount, value, requestId);
            await this.engine.DeleteTemplatesbyDomainAsync(currentAccount.EngagementAccount, value, requestId);

            await this.store.DeleteDomainAsync(currentAccount.EngagementAccount, value);
            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }

        private void ValidateDomain(string domain)
        {
            var length = domain?.Length ?? 0;
            Validator.IsTrue<ArgumentException>(!string.IsNullOrEmpty(domain), nameof(domain), "Domain cannot be null or empty string.");
            Validator.IsTrue<ArgumentException>(
                length >= EmailConstant.DomainMinLength && length <= EmailConstant.DomainMaxLength,
                nameof(Domain),
                "Invalid Domain. Length should be between {0} and {1}",
                EmailConstant.DomainMinLength,
                EmailConstant.DomainMaxLength);
            Validator.IsTrue<ArgumentException>(Uri.CheckHostName(domain) != UriHostNameType.Unknown, nameof(domain), "Invalid Domain.");
        }

        private async Task ValidateDomainExistAsync(string account)
        {
            var domainList = await this.store.ListDomainsByAccountAsync(account, new DbContinuationToken(null), -1);
            Validator.IsTrue<ArgumentException>(domainList != null && domainList.Domains != null && domainList.Domains.Count > 0, nameof(domainList), "Please create domains first.");
        }
    }
}
