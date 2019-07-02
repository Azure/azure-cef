// <copyright file="ChannelInvalidException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.Common
{
    public class ChannelInvalidException : Exception
    {
        public ChannelInvalidException(string message)
            : base(message)
        {
        }
    }
}
