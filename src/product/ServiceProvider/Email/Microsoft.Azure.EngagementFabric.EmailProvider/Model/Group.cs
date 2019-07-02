// <copyright file="Group.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Newtonsoft.Json;
using GroupContract = Microsoft.Azure.EngagementFabric.Email.Common.Contract.Group;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class Group
    {
        public Group()
        {
            this.Properties = new PropertyCollection<string>();
        }

        [JsonProperty(PropertyName = "GroupName", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "Emails", Required = Required.Always)]
        public List<string> Emails { get; set; }

        [JsonIgnore]
        public string EngagementAccount { get; set; }

        [JsonIgnore]
        public PropertyCollection<string> Properties { get; set; }

        [JsonIgnore]
        public string NextLink { get; set; }

        public GroupContract ToContract()
        {
            return new GroupContract
            {
                Name = this.Name,
                Emails = this.Emails,
                Properties = this.Properties
            };
        }
    }
}
