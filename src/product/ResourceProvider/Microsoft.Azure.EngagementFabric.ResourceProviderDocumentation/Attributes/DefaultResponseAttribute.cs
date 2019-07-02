// <copyright file="DefaultResponseAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Net;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes
{
    /// <summary>
    /// Indicates the schema of response
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DefaultResponseAttribute : Attribute
    {
        public DefaultResponseAttribute(HttpStatusCode statusCode)
        {
            this.StatusCode = ((int)statusCode).ToString();
        }

        public DefaultResponseAttribute(int statusCode)
        {
            this.StatusCode = statusCode.ToString();
        }

        public string StatusCode { get; }
    }
}
