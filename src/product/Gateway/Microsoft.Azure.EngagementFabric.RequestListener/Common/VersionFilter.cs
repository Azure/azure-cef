// <copyright file="VersionFilter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.Azure.EngagementFabric.Common.Versioning;
using Microsoft.Azure.EngagementFabric.RequestListener.Manager;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Common
{
    public class VersionFilter : ActionFilterAttribute
    {
        public async override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                // Just make sure api-version is in url
                // Let exception handler handle any exception
                ApiVersionHelper.GetApiVersion(actionContext.Request.RequestUri);
            }
            catch
            {
                var account = RequestHelper.ParseAccount(actionContext.Request);
                var subscriptionId = await RequestHelper.GetSubscriptionId(account);

                MetricManager.Instance.LogRequestFailed4xx(1, account, subscriptionId, string.Empty);
                throw;
            }

            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }
    }
}
