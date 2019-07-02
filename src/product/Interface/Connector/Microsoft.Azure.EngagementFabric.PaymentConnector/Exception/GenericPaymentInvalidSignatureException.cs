// <copyright file="GenericPaymentInvalidSignatureException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public abstract class GenericPaymentInvalidSignatureException : PaymentChannelExceptionBase
    {
        public GenericPaymentInvalidSignatureException(string channel, string expectedSign, string response)
            : base(
                  channel,
                  ErrorCodeEnum.INVALID_RESPONSE,
                  $"Signature of response does not match the expected value '{expectedSign}', response: {response}")
        {
        }
    }
}
