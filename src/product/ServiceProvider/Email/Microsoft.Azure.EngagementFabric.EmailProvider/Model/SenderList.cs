// <copyright file="SenderList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class SenderList
    {
        [JsonProperty("Total")]
        public int Total { get; set; }

        [JsonProperty("Senders")]
        public List<Sender> SenderAddresses { get; set; }

        [JsonIgnore]
        public DbContinuationToken NextLink { get; set; }
    }
}
