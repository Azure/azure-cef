// <copyright file="IAdminStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.RequestListener.Store
{
    internal interface IAdminStoreFactory
    {
        IAdminStore GetStore();
    }
}
