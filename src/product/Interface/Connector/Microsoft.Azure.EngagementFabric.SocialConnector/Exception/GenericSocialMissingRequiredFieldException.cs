// <copyright file="GenericSocialMissingRequiredFieldException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialConnector
{
    public abstract class GenericSocialMissingRequiredFieldException : SocialChannelExceptionBase
    {
        public GenericSocialMissingRequiredFieldException(string channel, string field, string response)
            : base(
                  channel,
                  ErrorCodeEnum.INVALID_RESPONSE,
                  $"Missing required field '{field}', response: {response}")
        {
        }
    }
}
