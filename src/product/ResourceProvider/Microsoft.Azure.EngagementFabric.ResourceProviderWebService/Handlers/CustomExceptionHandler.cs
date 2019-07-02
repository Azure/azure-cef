// <copyright file="CustomExceptionHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;
using Swashbuckle.Swagger.Annotations;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Handlers
{
    internal class CustomExceptionHandler : IExceptionHandler
    {
        private readonly StatelessServiceContext serviceContext;
        private readonly bool fullExceptionMessage;

        public CustomExceptionHandler(StatelessServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;

            var azureEnvironmentName = this.serviceContext
                .CodePackageActivationContext
                .GetConfig<string>("ResourceProviderWebService", "AzureEnvironmentName");

#if DEBUG
            this.fullExceptionMessage = azureEnvironmentName == "Local" || azureEnvironmentName == "Dogfood";
#else
            this.fullExceptionMessage = false;
#endif
        }

        public async Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            var actionName = "n/a";
            if (context.ExceptionContext.ActionContext != null)
            {
                var attribute = context.ExceptionContext.ActionContext.ActionDescriptor
                    .GetCustomAttributes<SwaggerOperationAttribute>()
                    .FirstOrDefault();

                actionName = attribute?.OperationId ?? context.ExceptionContext.ActionContext.ActionDescriptor.ActionName;
            }

            var controllerName = "n/a";
            if (context.ExceptionContext.ControllerContext != null)
            {
                controllerName = context.ExceptionContext.ControllerContext.ControllerDescriptor.ControllerType.Name;
            }

            var exception = context.Exception;
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            if (exception is ResourceNotFoundException
                || exception is ResourceAlreadyExistsException
                || exception is UnsupportedApiVersionException
                || exception is InvalidArgumentException)
            {
                ResourceProviderEventSource.Current.Info(
                    context.Request.GetRequestId() ?? "n/a",
                    controllerName,
                    actionName,
                    OperationStates.Failed,
                    exception.Message ?? string.Empty);
            }
            else
            {
                ResourceProviderEventSource.Current.ErrorException(
                    context.Request.GetRequestId() ?? "n/a",
                    controllerName,
                    actionName,
                    OperationStates.Failed,
                    string.Empty,
                    exception);
            }

            var content = new CloudError
            {
                Error = new CloudErrorBody
                {
                    Code = exception.GetType().Name,
                    Message = this.fullExceptionMessage ? exception.ToString() : exception.Message
                }
            };

            context.Result = new ResponseMessageResult(
                context.Request.CreateResponse(
                    HttpStatusCode.BadRequest,
                    content));

            await Task.CompletedTask;
        }
    }
}
