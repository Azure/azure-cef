// <copyright file="GenericPaymentMissingRequiredFieldException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public abstract class GenericPaymentMissingRequiredFieldException : PaymentChannelExceptionBase
    {
        public GenericPaymentMissingRequiredFieldException(string channel, string field, string response)
            : base(
                  channel,
                  ErrorCodeEnum.INVALID_RESPONSE,
                  $"Missing required field '{field}', response: {response}")
        {
        }
    }
}
