// <copyright file="GroupResultState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public enum GroupResultState
    {
        [EnumMember(Value = "PARTIALLY UPDATED")]
        PartiallyUpdated,

        [EnumMember(Value = "UPDATED")]
        Updated,

        [EnumMember(Value = "NO UPDATE")]
        NoUpdate
    }
}
