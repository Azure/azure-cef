// <copyright file="SASAuthorizeAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.EngagementFabric.Common.Extension;

namespace Microsoft.Azure.EngagementFabric.Common.Authorize
{
    public class SASAuthorizeAttribute : AuthorizeAttribute
    {
        public const string ConfigurationPropertyKey = "SASAuthorizeAttribute.GetKeysAsync";
        public const string AuthenticationFailureHandlerKey = "SASAuthorizeAttribute.TraceException";

        public delegate Task<IEnumerable<KeyValuePair<string, string>>> GetKeysAsyncFunc(string accountName, IEnumerable<string> keyNames);

        public delegate Task OnAuthenticationFailed(Exception exception, HttpRequestMessage request);

        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                var getKeysAsync = actionContext.GetControllerConfiguration<GetKeysAsyncFunc>(ConfigurationPropertyKey);
                if (getKeysAsync == null)
                {
                    throw new UnauthorizedAccessException("No key store");
                }

                if (actionContext.Request.Headers.Authorization == null)
                {
                    throw new UnauthorizedAccessException("Missing header 'Authorization'");
                }

                if (actionContext.Request.Headers.Authorization.Scheme != SASHelper.Schema)
                {
                    throw new UnauthorizedAccessException("Invalid authorization schema");
                }

                IEnumerable<string> accounts;
                if (!actionContext.Request.Headers.TryGetValues("Account", out accounts))
                {
                    throw new UnauthorizedAccessException("Missing header 'Account'");
                }

                var keyNames = this.Roles.Split(';');

                // Call 'AdminStore.GetKeysAsync' to retrieve keys
                var keyPairs = await getKeysAsync(accounts.First(), keyNames);
                if (keyPairs == null)
                {
                    throw new UnauthorizedAccessException("No authorize keys");
                }

                SASHelper.ValidateToken(actionContext.Request.Headers.Authorization.Parameter, keyPairs);
            }
            catch (Exception ex)
            {
                actionContext.GetControllerConfiguration<OnAuthenticationFailed>(AuthenticationFailureHandlerKey)?.Invoke(ex, actionContext.Request);

                if (ex is UnauthorizedAccessException || ex is SASInvalidException)
                {
                    throw;
                }
                else
                {
                    throw new UnauthorizedAccessException($"Internal exception: {ex.Message}");
                }
            }
        }
    }
}