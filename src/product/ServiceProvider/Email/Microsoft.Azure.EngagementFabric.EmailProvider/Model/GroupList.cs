// <copyright file="GroupList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class GroupList
    {
        [JsonProperty("Total")]
        public int Total { get; set; }

        [JsonProperty("GroupList")]
        public List<GroupInfo> GroupNames
        {
            get
            {
                return this.Groups?.Select(g => new GroupInfo
                {
                    Name = g.Name,
                    Description = g.Description
                }).ToList();
            }
        }

        [JsonIgnore]
        public List<Group> Groups { get; set; }

        [JsonIgnore]
        public DbContinuationToken NextLink { get; set; }

        public class GroupInfo
        {
            [JsonProperty(PropertyName = "GroupName")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "Description")]
            public string Description { get; set; }
        }
    }
}
