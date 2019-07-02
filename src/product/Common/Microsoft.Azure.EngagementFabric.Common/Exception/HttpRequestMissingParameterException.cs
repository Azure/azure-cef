// <copyright file="HttpRequestMissingParameterException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net.Http;

namespace Microsoft.Azure.EngagementFabric.Common
{
    public class HttpRequestMissingParameterException : HttpRequestException
    {
        public HttpRequestMissingParameterException(string parameterName)
            : base($"Missing parameter '{parameterName}'")
        {
            this.ParameterName = parameterName;
        }

        public string ParameterName { get; }
    }
}
