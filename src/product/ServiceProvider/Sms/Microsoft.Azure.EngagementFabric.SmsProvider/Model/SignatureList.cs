// <copyright file="SignatureList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class SignatureList
    {
        [JsonProperty("Total")]
        public int Total { get; set; }

        [JsonProperty("Signatures")]
        public List<Signature> Signatures { get; set; }

        [JsonIgnore]
        public DbContinuationToken NextLink { get; set; }
    }
}
