// <copyright file="ResourceState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public enum ResourceState
    {
        Unknown,
        Active,
        Pending,
        Forbidden,
        Disabled
    }
}
