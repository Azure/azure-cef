// <copyright file="ApiVersionHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Web;

namespace Microsoft.Azure.EngagementFabric.Common.Versioning
{
    public static class ApiVersionHelper
    {
        public static readonly ApiVersion CurrentApiVersion = GetVersion(ApiVersionConstants.MaxSupportedApiVersion);

        public static ApiVersion GetApiVersion(Uri requestUri)
        {
            var apiVersionString = GetApiVersionString(requestUri);
            return GetVersion(apiVersionString);
        }

        public static string GetApiVersionString(Uri requestUri)
        {
            if (!string.IsNullOrEmpty(requestUri.Query))
            {
                var nameValues = HttpUtility.ParseQueryString(requestUri.Query);
                return nameValues[ApiVersionConstants.Name];
            }

            return null;
        }

        public static string GetApiVersionString(ApiVersion version)
        {
            switch (version)
            {
                case ApiVersion.One:
                    return ApiVersionConstants.VersionOne;
                default:
                    throw new ArgumentException(ApiVersionConstants.Name);
            }
        }

        public static string GetApiVersionQueryString(ApiVersion version)
        {
            string apiVersionString = GetApiVersionString(version);
            if (string.IsNullOrEmpty(apiVersionString))
            {
                return string.Empty;
            }
            else
            {
                return string.Join("=", ApiVersionConstants.Name, apiVersionString);
            }
        }

        public static ApiVersion GetVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                throw new ArgumentException($"{ApiVersionConstants.Name} is required");
            }

            if (version.Equals(ApiVersionConstants.VersionOne, StringComparison.OrdinalIgnoreCase))
            {
                return ApiVersion.One;
            }

            throw new ArgumentException($"Supported API version is {ApiVersionConstants.SupportedVersions}");
        }
    }
}
