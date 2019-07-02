// <copyright file="ErrorCodeHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.Sms.Common
{
    public static class ErrorCodeHelper
    {
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

        public static MessageState ConvertMessageStateFromRequestOutcome(RequestOutcome outcome)
        {
            switch (outcome)
            {
                case RequestOutcome.UNKNOWN:
                    return MessageState.UNKNOWN;
                case RequestOutcome.DELIVERING:
                    return MessageState.UNKNOWN;
                case RequestOutcome.TIMEOUT:
                    return MessageState.TIMEOUT;
                case RequestOutcome.CANCELLED:
                    return MessageState.FAILED_OPERATOR;
                case RequestOutcome.SUCCESS:
                    return MessageState.DELIVERED;
                case RequestOutcome.FAILED_OPERATOR:
                    return MessageState.FAILED_OPERATOR;
                case RequestOutcome.FAILED_UNAUTHORIZED:
                    return MessageState.FAILED_OPERATOR;
                case RequestOutcome.FAILED_DATA_CONTRACT:
                    return MessageState.FAILED_OPERATOR;
                case RequestOutcome.FAILED_OVER_SPEED:
                    return MessageState.FAILED_SEND_FILTER;
                case RequestOutcome.FAILED_MOBILE:
                    return MessageState.FAILED_MOBILE;
                case RequestOutcome.FAILED_CONTENT:
                    return MessageState.FAILED_OPERATOR;
                case RequestOutcome.FAILED_SIGN:
                    return MessageState.FAILED_SIGN;
                case RequestOutcome.FAILED_EXTENDED_CODE:
                    return MessageState.FAILED_OPERATOR;
                case RequestOutcome.FAILED_BALANCE:
                    return MessageState.FAILED_OPERATOR;
                case RequestOutcome.FAILED_UNKNOWN:
                    return MessageState.FAILED_UNKNOWN;
            }

            return MessageState.FAILED_UNKNOWN;
        }
    }
}
