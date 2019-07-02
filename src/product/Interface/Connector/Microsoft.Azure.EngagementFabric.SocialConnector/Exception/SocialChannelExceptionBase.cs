// <copyright file="SocialChannelExceptionBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common;

namespace Microsoft.Azure.EngagementFabric.SocialConnector
{
    public abstract class SocialChannelExceptionBase : Exception
    {
        private static readonly Dictionary<ErrorCodeEnum, ExceptionCreator> ExceptionMapping = new Dictionary<ErrorCodeEnum, ExceptionCreator>
        {
                  { ErrorCodeEnum.NOAUTH,             (message) => new UnauthorizedAccessException(message) }
        };

        private static readonly ExceptionCreator DefaultCreator = (message) => new Exception(message);

        public SocialChannelExceptionBase(
            string channel,
            ErrorCodeEnum errorCode,
            string errorMessage)
            : base(
                  $"SocialLogin channel exception\r\n" +
                  $"  Channel = {channel}\r\n" +
                  $"  ErrorCode = {errorCode.ToString()}\r\n" +
                  $"  ErrorMessage = {errorMessage}")
        {
            this.Channel = channel;
            this.ErrorCode = errorCode;
            this.ErrorMessage = errorMessage;
        }

        private delegate Exception ExceptionCreator(string message);

        public enum ErrorCodeEnum
        {
            TRANSPORT_ERROR,
            INVALID_RESPONSE,
            NOAUTH,
            TOKEN_TIMEOUT,
            MISSING_PARAMETER,
            INVALID_PARAMETER,
            BUSINESS_ERROR,
            CHANNEL_ERROR
        }

        public string Channel { get; private set; }

        public ErrorCodeEnum ErrorCode { get; private set; }

        public string ErrorMessage { get; private set; }

        public Exception ToGenericException()
        {
            ExceptionCreator creator;
            if (!ExceptionMapping.TryGetValue(this.ErrorCode, out creator))
            {
                creator = DefaultCreator;
            }

#if DEBUG
            return creator(this.ToString());
#else
            return creator(this.ErrorMessage);
#endif
        }
    }
}
