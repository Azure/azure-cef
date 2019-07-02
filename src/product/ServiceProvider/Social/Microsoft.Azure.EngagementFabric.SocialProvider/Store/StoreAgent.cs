// <copyright file="StoreAgent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.SocialProvider.Engine;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Store
{
    public class StoreAgent
    {
        public StoreAgent(
            ISocialStore userInfoStore,
            CloudStorageAccount storageAccount)
        {
            UserInfoStore = userInfoStore;
            StorageAccount = storageAccount;
        }

        public ISocialStore UserInfoStore { get; private set; }

        public CloudStorageAccount StorageAccount { get; set; }
    }
}
