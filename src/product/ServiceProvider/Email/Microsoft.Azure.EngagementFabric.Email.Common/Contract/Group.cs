// <copyright file="Group.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Collection;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class Group
    {
        public Group()
        {
            this.Properties = new PropertyCollection<string>();
        }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public List<string> Emails { get; set; }

        [DataMember]
        public PropertyCollection<string> Properties { get; set; }
    }
}
