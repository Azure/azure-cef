// <copyright file="GenericPaymentHttpResponseException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public abstract class GenericPaymentHttpResponseException : PaymentChannelExceptionBase
    {
        public GenericPaymentHttpResponseException(string channel, string content)
            : base(
                  channel,
                  ErrorCodeEnum.TRANSPORT_ERROR,
                  content)
        {
        }
    }
}
