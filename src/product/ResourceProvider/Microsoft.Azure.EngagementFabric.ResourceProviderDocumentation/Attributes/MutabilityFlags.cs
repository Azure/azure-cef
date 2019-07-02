// <copyright file="MutabilityFlags.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes
{
    [Flags]
    [JsonConverter(typeof(StringEnumConverter), true)]
    public enum MutabilityFlags
    {
        Create = 0x01,
        Read = 0x02,
        Update = 0x04
    }
}
