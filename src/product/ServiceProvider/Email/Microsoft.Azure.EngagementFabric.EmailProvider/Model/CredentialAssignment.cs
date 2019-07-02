// <copyright file="CredentialAssignment.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class CredentialAssignment
    {
        public CredentialAssignment()
        {
        }

        [JsonProperty(PropertyName = "Provider", Required = Required.Always)]
        public string Provider { get; set; }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public string ConnectorId { get; set; }

        [JsonProperty(PropertyName = "Enabled", Required = Required.Always)]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "Active", Required = Required.Always)]
        public bool Active { get; set; }

        [JsonIgnore]
        public ConnectorIdentifier ConnectorIdentifier
        {
            get
            {
                return new ConnectorIdentifier(this.Provider, this.ConnectorId);
            }
        }

        [JsonIgnore]
        public string EngagementAccount { get; set; }
    }
}
