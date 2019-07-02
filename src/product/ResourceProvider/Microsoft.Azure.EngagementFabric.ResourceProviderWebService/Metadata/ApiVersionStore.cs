// <copyright file="ApiVersionStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.EngagementFabric.Common;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata
{
    internal static class ApiVersionStore
    {
        public const string DefaultApiVersion = "2018-09-01-preview";
        public static readonly IEnumerable<string> DefaultAcceptableApiVersions = new[]
        {
            DefaultApiVersion
        };

        public static void ValidateApiVersion(string apiVersion, IEnumerable<string> supportedApiVersions = null)
        {
            if (!(supportedApiVersions ?? DefaultAcceptableApiVersions).Contains(apiVersion, StringComparer.OrdinalIgnoreCase))
            {
                throw new UnsupportedApiVersionException(apiVersion, supportedApiVersions);
            }
        }
    }
}
