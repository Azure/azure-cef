// -----------------------------------------------------------------------
// <copyright file="CustomExceptionHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.RequestListener.Manager;
using Microsoft.ServiceFabric.Services.Communication;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Common
{
    public class CustomExceptionHandler : IExceptionHandler
    {
        public async Task HandleAsync(ExceptionHandlerContext context, CancellationToken cancellationToken)
        {
            var statusCode = HttpStatusCode.InternalServerError;
            var errorMessage = string.Empty;
            var exception = context.Exception;

            // Loop to find the first exception
            while (exception.InnerException != null)
            {
                exception = exception.InnerException;
            }

            errorMessage = exception?.Message;

            var headers = new Dictionary<string, string>();

            // CEF specified exceptions
            if (exception is HttpRequestInvalidParameterTypeException ||
                exception is HttpRequestMissingParameterException ||
                exception is ServiceException)
            {
                statusCode = HttpStatusCode.BadRequest;
                errorMessage = "Invalid request. Please check your request body.";
            }
            else if (exception is ResourceNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (exception is SASInvalidException)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }
            else if (exception is QuotaExceededException)
            {
                statusCode = (HttpStatusCode)429;
                headers.Add(Constants.QuotaRemainingHeader, (exception as QuotaExceededException).Remaining.ToString());
            }
            else if (exception is AccountDisabledException)
            {
                statusCode = HttpStatusCode.Forbidden;
            }

            // Common exceptions
            else if (exception is ArgumentException ||
                exception is ArgumentOutOfRangeException ||
                exception is ArgumentNullException ||
                exception is InvalidOperationException ||
                exception is JsonSerializationException ||
                exception is InvalidDataContractException)
            {
                statusCode = HttpStatusCode.BadRequest;
            }
            else if (exception is NotImplementedException ||
                exception is NotSupportedException)
            {
                statusCode = HttpStatusCode.NotAcceptable;
            }
            else if (exception is ObjectNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }

            // Add metric for failure
            if ((int)statusCode >= 400 && (int)statusCode < 600)
            {
                var account = RequestHelper.ParseAccount(context.Request);
                var subscriptionId = await RequestHelper.GetSubscriptionId(account);
                var provider = Regex.Match(context.Request.RequestUri.AbsolutePath, "/services/(.+)/").Groups[1].Value;
                if ((int)statusCode < 500)
                {
                    MetricManager.Instance.LogRequestFailed4xx(1, account, subscriptionId, provider);
                }
                else
                {
                    MetricManager.Instance.LogRequestFailed5xx(1, account, subscriptionId, provider);
                }
            }

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                // Trace the full exception
                var trackingId = RequestHelper.ParseTrackingId(context.Request);
                GatewayEventSource.Current.ErrorException(trackingId, exception.Source, nameof(this.HandleAsync), OperationStates.Failed, string.Empty, context.Exception);

                // Convert 500 to 400
                statusCode = HttpStatusCode.BadRequest;
            }

#if DEBUG
            var response = context.Request.CreateResponse(statusCode, new FailResponseModel(context.Exception.ToString()));
#else
            var response = context.Request.CreateResponse(statusCode, new FailResponseModel(errorMessage));
#endif

            foreach (var pair in headers)
            {
                response.Headers.Add(pair.Key, pair.Value);
            }

            context.Result = new ResponseMessageResult(response);
        }
    }
}