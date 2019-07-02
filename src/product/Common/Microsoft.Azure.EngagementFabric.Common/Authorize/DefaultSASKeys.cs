// <copyright file="DefaultSASKeys.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.Common.Authorize
{
    public static class DefaultSASKeys
    {
        public const string DefaultFullKeyName = "full";
        public const string DefaultDeviceKeyName = "device";

        public const int DefaultKeyLength = 32;

        public static readonly IEnumerable<string> DefaultKeyNames = new[]
        {
            DefaultFullKeyName,
            DefaultDeviceKeyName
        };
    }
}
