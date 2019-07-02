// <copyright file="ApiVersionConstants.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.Common.Versioning
{
    public class ApiVersionConstants
    {
        public const string Name = "api-version";
        public const string VersionOne = "2018-10-01";

        public const string MaxSupportedApiVersion = VersionOne;

        public static readonly string SupportedVersions = string.Join(
            ",",
            VersionOne);
    }
}
