// <copyright file="UnsupportedApiVersionException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Common
{
    [Serializable]
    public class UnsupportedApiVersionException : Exception
    {
        public UnsupportedApiVersionException(string apiVersion, IEnumerable<string> supportedApiVersions)
            : base($"The api version '{apiVersion}' is invalid. The supported versions are {string.Join(", ", supportedApiVersions)}")
        {
        }

        protected UnsupportedApiVersionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
