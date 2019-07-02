// <copyright file="IAdminStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Store
{
    internal interface IAdminStore
    {
        Task<KeyValuePair<string, string>[]> GetKeysAsync(string accountName, IEnumerable<string> keyNames);
    }
}
