// <copyright file="GenericPaymentChannelException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public abstract class GenericPaymentChannelException : PaymentChannelExceptionBase
    {
        public GenericPaymentChannelException(string channel, ErrorCodeEnum errorCode, string errorMessage)
            : base(
                  channel,
                  errorCode,
                  errorMessage)
        {
        }
    }
}
