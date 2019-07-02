// <copyright file="ISmsStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Store
{
    public interface ISmsStoreFactory
    {
        ISmsStore GetStore();
    }
}
