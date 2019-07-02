// <copyright file="AccountController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.EmailProvider.Model;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Controller
{
    public sealed partial class OperationController
    {
        #region Account

        [HttpPost]
        [Route("admin/accounts")]
        public async Task<ServiceProviderResponse> CreateOrUpdateAccount(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            [FromBody] Account request)
        {
            Validator.ArgumentNotNull(request, nameof(request));
            Validator.ArgumentNotNullOrEmpty(request.EngagementAccount, nameof(request.EngagementAccount));

            var account = await this.CreateOrUpdateAccountAsync(request);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, account);
        }

        [HttpGet]
        [Route("admin/accounts/{account}")]
        public async Task<ServiceProviderResponse> GetAccount(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            var result = await this.GetAccountAsync(currentAccount.EngagementAccount);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, result);
        }

        [HttpDelete]
        [Route("admin/accounts/{account}")]
        public async Task<ServiceProviderResponse> DeleteAccount(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account)
        {
            var result = await this.GetAccountAsync(account);
            if (result == null)
            {
                return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
            }

            await this.DeleteAccountAsync(result.EngagementAccount);
            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #region Credential Assignment

        [HttpPost]
        [Route("admin/accounts/{account}/credentialassignments")]
        public async Task<ServiceProviderResponse> CreateOrUpdateCredentialAssignments(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account,
            [FromBody] CredentialAssignment request)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            Validator.ArgumentNotNull(request, nameof(request));
            Validator.ArgumentNotNullOrEmpty(request.Provider, nameof(request.Provider));
            Validator.ArgumentNotNullOrEmpty(request.ConnectorId, nameof(request.ConnectorId));

            request.EngagementAccount = currentAccount.EngagementAccount;
            await this.credentialManager.CreateOrUpdateCredentialAssignmentAsync(request);

            var response = await this.credentialManager.ListCredentialAssignmentsByAccountAsync(currentAccount.EngagementAccount, false);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [Route("admin/accounts/{account}/credentialassignments")]
        public async Task<ServiceProviderResponse> ListCredentialAssignments(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            var response = await this.credentialManager.ListCredentialAssignmentsByAccountAsync(currentAccount.EngagementAccount, false);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, response);
        }

        [HttpDelete]
        [Route("admin/accounts/{account}/credentialassignments/{provider}/{id}")]
        public async Task<ServiceProviderResponse> DeleteCredentialAssignments(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account,
            string provider,
            string id)
        {
            var currentAccount = await EnsureAccount(account, requestId);

            await this.credentialManager.DeleteCredentialAssignmentsAsync(currentAccount.EngagementAccount, new ConnectorIdentifier(provider, id));
            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

        #region Internal

        public async Task<Account> GetAccountAsync(string account)
        {
            return await this.store.GetAccountAsync(account);
        }

        public async Task<Account> CreateOrUpdateAccountAsync(Account account)
        {
            var updated = await this.store.CreateOrUpdateAccountAsync(account);
            await this.reportManager.OnAccountCreatedOrUpdatedAsync(updated.EngagementAccount);

            return updated;
        }

        public async Task DeleteAccountAsync(string account)
        {
            // #1 cleanup db resource of Template, Group, Sender
            await this.store.DeleteTemplatesAsync(account);
            await this.store.DeleteGroupsAsync(account);
            await this.store.DeleteSendersAsync(account);

            // #2 cleanup Email Account resources
            try
            {
                await this.credentialManager.CleanupEmailAccountAsync(account);
            }
            catch (Exception ex)
            {
                // Catch the exception because even though cleanup in provider failed, we also want to delete the db resource
                EmailProviderEventSource.Current.ErrorException(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.DeleteAccountAsync), OperationStates.Failed, $"Failed to clean up EmailAccount {account} in provider.", ex);
            }

            // #3 cleanup db resource of Domain, CredentialAssignment
            await this.store.DeleteDomainsAsync(account);
            await this.credentialManager.DeleteCredentialAssignmentsAsync(account, null);

            // #4 delete table
            await this.reportManager.OnAccountDeletedAsync(account);

            // #5 delete EngagementAccount
            await this.store.DeleteAccountAsync(account);
        }

        #endregion
    }
}
