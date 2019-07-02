// <copyright file="IEmailStoreFactory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Store
{
    public interface IEmailStoreFactory
    {
        IEmailStore GetStore();
    }
}
