// <copyright file="Sender.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class Sender
    {
        [JsonProperty(PropertyName = "SenderAddrID", NullValueHandling = NullValueHandling.Ignore)]
        public string SenderAddrID { get; set; }

        [JsonProperty(PropertyName = "SenderAddr", Required = Required.Always)]
        public string SenderAddress { get; set; }

        [JsonProperty(PropertyName = "ForwardAddr", Required = Required.Always)]
        public string ForwardAddress { get; set; }

        [JsonIgnore]
        public string EngagementAccount { get; set; }

        [JsonIgnore]
        public PropertyCollection<string> Properties { get; set; }

        [JsonIgnore]
        public EmailAddress SenderEmailAddress
        {
            get
            {
                return new EmailAddress(this.SenderAddress);
            }
        }

        [JsonIgnore]
        public EmailAddress ForwardEmailAddress
        {
            get
            {
                return new EmailAddress(this.ForwardAddress);
            }
        }

        [JsonIgnore]
        public string Domain
        {
            get
            {
                return this.SenderEmailAddress?.Host;
            }
        }

        public SenderAddress ToContract()
        {
            return new SenderAddress
            {
                SenderdAddress = SenderEmailAddress,
                ForwardAddress = ForwardEmailAddress,
                Properties = this.Properties
            };
        }
    }
}
