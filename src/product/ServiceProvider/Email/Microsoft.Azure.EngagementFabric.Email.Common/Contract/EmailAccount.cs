// <copyright file="EmailAccount.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Collection;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class EmailAccount
    {
        public EmailAccount()
        {
            this.Properties = new PropertyCollection<string>();
        }

        [DataMember]
        public string EngagementAccount { get; set; }

        [DataMember]
        public List<string> Domains { get; set; }

        [DataMember]
        public PropertyCollection<string> Properties { get; set; }
    }
}
