// <copyright file="GenericSocialMismatchFieldException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialConnector
{
    public abstract class GenericSocialMismatchFieldException : SocialChannelExceptionBase
    {
        public GenericSocialMismatchFieldException(string channel, string key, string expectedValue, string response)
            : base(
                  channel,
                  ErrorCodeEnum.INVALID_RESPONSE,
                  $"Field '{key}' of response does not match the expected value '{expectedValue}', response: {response}")
        {
        }
    }
}
