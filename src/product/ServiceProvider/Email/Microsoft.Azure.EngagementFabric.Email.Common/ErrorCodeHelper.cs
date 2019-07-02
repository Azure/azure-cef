// <copyright file="ErrorCodeHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.Email.Common
{
    public static class ErrorCodeHelper
    {
        /// <summary>
        /// Mapping non 200 response to RequestOutcome (by now only common outcome and no Email specified outcome)
        /// </summary>
        /// <param name="webException">WebException</param>
        /// <returns>RequestOutcome</returns>
        public static RequestOutcome GetOutcomeForWebException(WebException webException)
        {
            if (webException.Status == WebExceptionStatus.ProtocolError)
            {
                HttpWebResponse webResponse = (HttpWebResponse)webException.Response;

                switch (webResponse.StatusCode)
                {
                    case HttpStatusCode.BadRequest:
                        return RequestOutcome.FAILED_DATA_CONTRACT;
                    case HttpStatusCode.Unauthorized:
                        return RequestOutcome.FAILED_UNAUTHORIZED;
                    case HttpStatusCode.InternalServerError:
                    case HttpStatusCode.ServiceUnavailable:
                    case HttpStatusCode.BadGateway:
                    case HttpStatusCode.GatewayTimeout:
                        return RequestOutcome.FAILED_UNKNOWN;
                }
            }

            return RequestOutcome.FAILED_UNKNOWN;
        }

        /// <summary>
        /// Mapping RequestOutcome to MessageState
        /// </summary>
        /// <param name="outcome">RequestOutcome</param>
        /// <returns>MessageState</returns>
        public static MessageState ConvertMessageStateFromRequestOutcome(RequestOutcome outcome)
        {
            switch (outcome)
            {
                case RequestOutcome.UNKNOWN:
                case RequestOutcome.DELIVERING:
                    return MessageState.UNKNOWN;
                case RequestOutcome.SUCCESS:
                    return MessageState.DELIVERED;
                case RequestOutcome.TIMEOUT:
                    return MessageState.TIMEOUT;
                case RequestOutcome.CANCELLED:
                case RequestOutcome.FAILED_OPERATOR:
                case RequestOutcome.FAILED_UNAUTHORIZED:
                case RequestOutcome.FAILED_DATA_CONTRACT:
                 case RequestOutcome.FAILED_CONTENT:
                case RequestOutcome.FAILED_BALANCE:
                    return MessageState.FAILED_OPERATOR;
            }

            return MessageState.FAILED_UNKNOWN;
        }
    }
}
