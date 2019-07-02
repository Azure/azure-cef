// <copyright file="BaseController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Linq;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Controllers
{
    /// <summary>
    /// Base API controller providing the log facility
    /// </summary>
    public class BaseController : ApiController
    {
        /// <summary>
        /// The key of configuration property holding the StatelessServiceContext
        /// </summary>
        public const string ServiceContextKey = "serviceContext";

        /// <summary>
        /// Trace message on action begin
        /// </summary>
        /// <param name="message">Trace message</param>
        protected void LogActionBegin(string message = null)
        {
            var attribute = this.ActionContext.ActionDescriptor
                .GetCustomAttributes<SwaggerOperationAttribute>()
                .FirstOrDefault();

            object subscriptionId, resourceGroupName, accountName, channelName, apiVersion;
            this.ActionContext.ActionArguments.TryGetValue("subscriptionId", out subscriptionId);
            this.ActionContext.ActionArguments.TryGetValue("resourceGroupName", out resourceGroupName);
            this.ActionContext.ActionArguments.TryGetValue("accountName", out accountName);
            this.ActionContext.ActionArguments.TryGetValue("channelName", out channelName);
            this.ActionContext.ActionArguments.TryGetValue("apiVersion", out apiVersion);

            ResourceProviderEventSource.Current.ActionBegin(
                this.Request.GetRequestId() ?? "n/a",
                attribute?.OperationId ?? this.ActionContext.ActionDescriptor.ActionName,
                subscriptionId as string ?? "n/a",
                resourceGroupName as string ?? "n/a",
                accountName as string ?? "n/a",
                channelName as string ?? "n/a",
                apiVersion as string ?? "n/a",
                message ?? string.Empty);
        }

        /// <summary>
        /// Trace message on action end
        /// </summary>
        /// <param name="message">Trace message</param>
        protected void LogActionEnd(string message = null)
        {
            var attribute = this.ActionContext.ActionDescriptor
                .GetCustomAttributes<SwaggerOperationAttribute>()
                .FirstOrDefault();

            ResourceProviderEventSource.Current.ActionEnd(
                this.Request.GetRequestId() ?? "n/a",
                attribute?.OperationId ?? this.ActionContext.ActionDescriptor.ActionName,
                message ?? string.Empty);
        }
    }
}
