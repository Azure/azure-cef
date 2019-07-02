// <copyright file="KeyRank.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    /// <summary>
    /// The rank of the EngagementFabric account key
    /// </summary>
    [JsonConverter(typeof(RestrictedStringEnumConverter))]
    public enum KeyRank
    {
        /// <summary>
        /// Primary key
        /// </summary>
        PrimaryKey,

        /// <summary>
        /// Secondary key
        /// </summary>
        SecondaryKey
    }
}
