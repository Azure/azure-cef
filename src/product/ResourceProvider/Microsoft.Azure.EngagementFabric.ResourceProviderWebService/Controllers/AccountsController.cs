// <copyright file="AccountsController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Examples;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Controllers
{
    /// <summary>
    /// Controller for the EngagementFabric account resource operations
    /// </summary>
    public class AccountsController : BaseController
    {
        private readonly IAccountManager accountManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountsController"/> class.
        /// </summary>
        /// <param name="accountManager">The account manager</param>
        public AccountsController(IAccountManager accountManager)
        {
            this.accountManager = accountManager;
        }

        /// <summary>
        /// Create or Update the EngagementFabric account
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="account">The EngagementFabric account description</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>The full EngagementFabric Account description</returns>
        [HttpPut]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}")]
        [SwaggerOperation("Accounts_CreateOrUpdate")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(AccountsCreateOrUpdateExample))]
        public async Task<Account> CreateOrUpdateAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [FromBody] Account account,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));
            Validator.ArgumentNotNullOrrWhiteSpace(account.SKU.Name, account.SKU.Name);
            Validator.ArgumentNotNullOrrWhiteSpace(account.Location, nameof(account.Location));

            this.LogActionBegin(
                $"Location = {account.Location}\n" +
                $"SKU = {account.SKU?.Name}\n" +
                $"Tags = {this.SerializeTags(account.Tags)}");

            var result = await this.accountManager.CreateOrUpdateAccountAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                accountName,
                account);

            this.LogActionEnd(
                $"Resource ID = {result.Id}\n" +
                $"Location = {result.Location}\n" +
                $"SKU = {result.SKU?.Name}\n" +
                $"Tags = {this.SerializeTags(result.Tags)}");
            return result;
        }

        /// <summary>
        /// Update EngagementFabric account
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">Account name</param>
        /// <param name="accountPatch">The account patch</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>The full Account description</returns>
        [HttpPatch]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}")]
        [SwaggerOperation("Accounts_Update")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(AccountsUpdateExample))]
        public async Task<Account> UpdateAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [FromBody] AccountPatch accountPatch,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));

            this.LogActionBegin($"Tags = {this.SerializeTags(accountPatch.Tags)}");

            var result = await this.accountManager.UpdateAccountAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                accountName,
                accountPatch);

            this.LogActionEnd($"Tags = {this.SerializeTags(result.Tags)}");
            return result;
        }

        /// <summary>
        /// Delete the EngagementFabric account
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>n/a</returns>
        [HttpDelete]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}")]
        [SwaggerOperation("Accounts_Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NoContent)]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(AccountsDeleteExample))]
        public async Task<IHttpActionResult> DeleteAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));

            this.LogActionBegin();

            var found = await this.accountManager.DeleteAccountAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                accountName);

            this.LogActionEnd($"Found account to be deleted: {found}");
            if (found)
            {
                return this.Ok();
            }
            else
            {
                return this.StatusCode(HttpStatusCode.NoContent);
            }
        }

        /// <summary>
        /// Get the EngagementFabric account
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>The full EngagementFabric account description</returns>
        [HttpGet]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}")]
        [SwaggerOperation("Accounts_Get")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(AccountsGetExample))]
        public async Task<Account> GetAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));

            this.LogActionBegin();

            var result = await this.accountManager.GetAccountAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                accountName);

            this.LogActionEnd(
                $"Resource ID = {result.Id}\n" +
                $"Location = {result.Location}\n" +
                $"SKU = {result.SKU.Name}\n" +
                $"Tags = {this.SerializeTags(result.Tags)}");
            return result;
        }

        /// <summary>
        /// List the EngagementFabric accounts in given subscription
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>List of the full EngagementFabric account descriptions</returns>
        [HttpGet]
        [Route("subscriptions/{subscriptionId}/providers/" + NameStore.FullyQualifiedAccountResourceType)]
        [SwaggerOperation("Accounts_List")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [NonPageable]
        [Example(typeof(AccountsListExample))]
        public async Task<AccountList> ListAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));

            this.LogActionBegin();

            var result = new AccountList
            {
                Accounts = await this.accountManager.ListAccountsAsync(
                    this.Request.GetRequestId(),
                    subscriptionId)
            };

            this.LogActionEnd($"Total {result.Accounts.Count()} accounts returned");
            return result;
        }

        /// <summary>
        /// List EngagementFabric accounts in given resource group
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>List of the full EngagementFabric account descriptions</returns>
        [HttpGet]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType)]
        [SwaggerOperation("Accounts_ListByResourceGroup")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [NonPageable]
        [Example(typeof(AccountsListByResourceGroupExample))]
        public async Task<AccountList> ListByResourceGroupAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));

            this.LogActionBegin();

            var result = new AccountList
            {
                Accounts = await this.accountManager.ListAccountsByResourceGroupAsync(
                    this.Request.GetRequestId(),
                    subscriptionId,
                    resourceGroupName)
            };

            this.LogActionEnd($"Total {result.Accounts.Count()} accounts returned");
            return result;
        }

        /// <summary>
        /// List keys of the EngagementFabric account
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>List of the key descriptions</returns>
        [HttpPost]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}/listKeys")]
        [SwaggerOperation("Accounts_ListKeys")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [NonPageable]
        [Example(typeof(AccountsListKeysExample))]
        public async Task<KeyDescriptionList> ListKeysAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));

            this.LogActionBegin();

            var result = new KeyDescriptionList
            {
                Keys = await this.accountManager.ListKeysAsync(
                    this.Request.GetRequestId(),
                    subscriptionId,
                    resourceGroupName,
                    accountName)
            };

            this.LogActionEnd($"Total {result.Keys.Count()} keys returned");
            return result;
        }

        /// <summary>
        /// Regenerate key of the EngagementFabric account
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="parameter">Parameters specifying the key to be regenerated</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>Description of the regenerated key</returns>
        [HttpPost]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}/regenerateKey")]
        [SwaggerOperation("Accounts_RegenerateKey")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(AccountsRegenerateKeyExample))]
        public async Task<KeyDescription> RegenerateKeyAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [FromBody] RegenerateKeyParameter parameter,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            if (!this.ModelState.IsValid)
            {
                var error = this.ModelState.Values
                    .SelectMany(s => s.Errors)
                    .FirstOrDefault();

                throw new InvalidArgumentException(error?.Exception?.Message ?? "Invalid model");
            }

            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));
            Validator.ArgumentNotNullOrrWhiteSpace(parameter.Name, nameof(parameter.Name));

            this.LogActionBegin(
                $"Key name = {parameter.Name}\n" +
                $"Key rank = {parameter.Rank}");

            var result = await this.accountManager.RegenerateKeyAsync(
                this.Request.GetRequestId(),
                subscriptionId,
                resourceGroupName,
                accountName,
                parameter);

            this.LogActionEnd();
            return result;
        }

        /// <summary>
        /// List available EngagementFabric channel types and functions
        /// </summary>
        /// <param name="subscriptionId">The subscription ID</param>
        /// <param name="resourceGroupName">The resource group name</param>
        /// <param name="accountName">The EngagementFabric account name</param>
        /// <param name="apiVersion">API version</param>
        /// <returns>List of the channel descriptions</returns>
        [HttpPost]
        [Route("subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/" + NameStore.FullyQualifiedAccountResourceType + "/{accountName}/listChannelTypes")]
        [SwaggerOperation("Accounts_ListChannelTypes")]
        [SwaggerResponse(0, Type = typeof(CloudError))]
        [DefaultResponse(0)]
        [Example(typeof(AccountsListChannelTypesExample))]
        public async Task<ChannelTypeDescriptionList> ListChannelTypesAsync(
            [GlobalParameter("subscriptionId")] string subscriptionId,
            [GlobalParameter("resourceGroupName")] string resourceGroupName,
            [GlobalParameter("accountName")] string accountName,
            [GlobalParameter("api-version"), FromQuery("api-version")] string apiVersion)
        {
            ApiVersionStore.ValidateApiVersion(apiVersion);
            Validator.ArgumentValidGuid(subscriptionId, nameof(subscriptionId));
            Validator.ArgumentNotNullOrrWhiteSpace(resourceGroupName, nameof(resourceGroupName));
            Validator.ArgumentNotNullOrrWhiteSpace(accountName, nameof(accountName));

            this.LogActionBegin();

            var result = await Task.FromResult(new ChannelTypeDescriptionList
            {
                Descriptions = ChannelTypeStore.Descriptions
            });

            this.LogActionEnd($"Total {result.Descriptions.Count()} channel types returned");
            return result;
        }

        private string SerializeTags(IReadOnlyDictionary<string, string> tags)
        {
            if (tags == null)
            {
                return "(null)";
            }
            else
            {
                return $"[{string.Join(", ", tags.Select(pair => $"{pair.Key} = {pair.Value}"))}]";
            }
        }
    }
}