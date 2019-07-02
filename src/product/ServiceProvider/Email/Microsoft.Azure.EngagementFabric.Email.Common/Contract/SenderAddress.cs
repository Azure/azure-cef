// <copyright file="SenderAddress.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Collection;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class SenderAddress
    {
        public SenderAddress()
        {
            this.Properties = new PropertyCollection<string>();
        }

        [DataMember]
        public EmailAddress SenderdAddress { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public EmailAddress ForwardAddress { get; set; }

        [DataMember]
        public PropertyCollection<string> Properties { get; set; }
    }
}
