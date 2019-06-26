﻿// <copyright file="TemplateList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class TemplateList
    {
        [JsonProperty("Total")]
        public int Total { get; set; }

        [JsonProperty("Templates")]
        public List<Template> Templates { get; set; }

        [JsonIgnore]
        public DbContinuationToken NextLink { get; set; }
    }
}
