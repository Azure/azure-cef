// <copyright file="GenericSocialHttpResponseException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialConnector
{
    public abstract class GenericSocialHttpResponseException : SocialChannelExceptionBase
    {
        public GenericSocialHttpResponseException(string channel, string content)
            : base(
                  channel,
                  ErrorCodeEnum.TRANSPORT_ERROR,
                  content)
        {
        }
    }
}
