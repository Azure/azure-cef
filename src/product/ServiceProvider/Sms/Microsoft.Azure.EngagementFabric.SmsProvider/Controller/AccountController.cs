// <copyright file="AccountController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Constant = Microsoft.Azure.EngagementFabric.Common.Constants;
using SmsConstant = Microsoft.Azure.EngagementFabric.SmsProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Controller
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
            Validator.ArgumentNotNull(request.AccountSettings, nameof(request.AccountSettings));

            // If provider is configured, verify its existence
            if (!string.IsNullOrEmpty(request.Provider))
            {
                var connector = await this.store.GetConnectorMetadataAsync(request.Provider);
                Validator.IsTrue<ArgumentException>(connector != null, nameof(request.Provider), "Provider '{0}' does not exist.", request.Provider);
            }

            var account = await this.CreateOrUpdateAccountAsync(request);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, account);
        }

        [HttpGet]
        [Route("admin/accounts/{account}")]
        public async Task<ServiceProviderResponse> GetAccount(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account)
        {
            var result = await this.GetAccountAsync(account);
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, result);
        }

        [HttpDelete]
        [Route("admin/accounts/{account}")]
        public async Task<ServiceProviderResponse> DeleteAccount(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account)
        {
            await ValidateAccount(account);
            await DeleteAccountAsync(account);
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
            await ValidateAccount(account);

            Validator.ArgumentNotNull(request, nameof(request));
            Validator.ArgumentNotNullOrEmpty(request.Provider, nameof(request.Provider));
            Validator.ArgumentNotNullOrEmpty(request.ConnectorId, nameof(request.ConnectorId));
            Validator.IsTrue<ArgumentException>(request.ChannelType != ChannelType.Invalid, nameof(request.ChannelType), "Invalid channel type.");

            var identifier = new ConnectorIdentifier(request.Provider, request.ConnectorId);
            var credential = await this.credentialManager.GetConnectorCredentialByIdAsync(identifier);
            Validator.IsTrue<ResourceNotFoundException>(credential != null, nameof(credential), "Credential '{0}' does not exist.", identifier);
            Validator.IsTrue<ArgumentException>(credential.ChannelType == request.ChannelType, nameof(request.ChannelType), "Credential '{0}' is for channel type '{1}' but not '{2}'", identifier, credential.ChannelType.ToString(), request.ChannelType.ToString());

            if (string.IsNullOrEmpty(request.ExtendedCode))
            {
                // Auto-assign account code
                var otherAccounts = await this.credentialManager.ListCredentialAssignmentsById(identifier, false);
                if (otherAccounts != null && otherAccounts.Count > 0)
                {
                    var last = otherAccounts.Where(a => a.ExtendedCode != null).OrderBy(a => a.ExtendedCode).LastOrDefault();
                    if (int.TryParse(last.ExtendedCode, out int code) && code < Math.Pow(10, SmsConstant.ExtendedCodeCompanyLength) - 1)
                    {
                        request.ExtendedCode = (code + 1).ToString().PadLeft(SmsConstant.ExtendedCodeCompanyLength, '0');
                    }
                    else
                    {
                        throw new ApplicationException($"Failed the generate new code. The last one is '{last.ExtendedCode}'");
                    }
                }
                else
                {
                    request.ExtendedCode = 1.ToString().PadLeft(SmsConstant.ExtendedCodeCompanyLength, '0');
                }
            }

            await this.credentialManager.CreateOrUpdateCredentialAssignmentAsync(request.ToDataContract(account));

            var assignmentList = await this.credentialManager.ListCredentialAssignmentsByAccountAsync(account, ChannelType.Both, false);
            var response = assignmentList?.Select(a => new CredentialAssignment(a)).ToList();
            return ServiceProviderResponse.CreateJsonResponse(HttpStatusCode.OK, response);
        }

        [HttpGet]
        [Route("admin/accounts/{account}/credentialassignments")]
        public async Task<ServiceProviderResponse> ListCredentialAssignments(
            [FromHeader(Constant.OperationTrackingIdHeader)] string requestId,
            string account)
        {
            await ValidateAccount(account);

            var assignmentList = await this.credentialManager.ListCredentialAssignmentsByAccountAsync(account, ChannelType.Both, false);
            var response = assignmentList?.Select(a => new CredentialAssignment(a)).ToList();
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
            await ValidateAccount(account);

            await this.credentialManager.DeleteCredentialAssignmentsAsync(account, new ConnectorIdentifier(provider, id));
            return ServiceProviderResponse.CreateResponse(HttpStatusCode.OK);
        }

        #endregion

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
            // Delete templates
            await this.store.DeleteTemplatesAsync(account);

            // Delete signatures
            await this.store.DeleteSignaturesAsync(account);

            // Delete credential assignments
            await this.credentialManager.DeleteCredentialAssignmentsAsync(account, null);

            // Delete inbound messages
            await this.inboundManager.OnAccountDeletedAsync(account);

            // Delete message history
            await this.reportManager.OnAccountDeletedAsync(account);

            // Delete account
            await this.store.DeleteAccountAsync(account);
        }
    }
}
