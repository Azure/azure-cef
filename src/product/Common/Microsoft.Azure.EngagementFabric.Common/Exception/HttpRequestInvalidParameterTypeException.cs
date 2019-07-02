// <copyright file="HttpRequestInvalidParameterTypeException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Net.Http;

namespace Microsoft.Azure.EngagementFabric.Common
{
    public class HttpRequestInvalidParameterTypeException : HttpRequestException
    {
        public HttpRequestInvalidParameterTypeException(string parameterName, Type parameterType)
            : base($"Failed to cast parameter '{parameterName}' to type '{parameterType.FullName}'")
        {
            this.ParameterName = parameterName;
            this.ParameterType = parameterType;
        }

        public string ParameterName { get; }

        public Type ParameterType { get; }
    }
}
