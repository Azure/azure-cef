// <copyright file="ISocialStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Store
{
    public interface ISocialStoreFactory
    {
        ISocialStore GetStore();
    }
}
