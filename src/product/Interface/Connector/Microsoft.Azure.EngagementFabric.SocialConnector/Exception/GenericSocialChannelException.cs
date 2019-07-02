// <copyright file="GenericSocialChannelException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialConnector
{
    public abstract class GenericSocialChannelException : SocialChannelExceptionBase
    {
        public GenericSocialChannelException(string channel, ErrorCodeEnum errorCode, string errorMessage)
            : base(
                  channel,
                  errorCode,
                  errorMessage)
        {
        }
    }
}
