// <copyright file="Credential.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.Common.Collection;
using Microsoft.Azure.EngagementFabric.EmailProvider.Credential;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class Credential
    {
        public Credential()
        {
        }

        public Credential(EmailConnectorCredential connectorCredential)
        {
            this.ConnectorName = connectorCredential.ConnectorName;
            this.ConnectorKey = connectorCredential.ConnectorId;
            this.ConnectorProperties = new PropertyCollection<string>(connectorCredential.ConnectorProperties);
        }

        [JsonProperty(PropertyName = "Provider", Required = Required.Always)]
        public string ConnectorName { get; set; }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public string ConnectorKey { get; set; }

        [JsonProperty(PropertyName = "Properties", Required = Required.Always)]
        public PropertyCollection<string> ConnectorProperties { get; set; }

        public EmailConnectorCredential ToConnectorCredential()
        {
            return new EmailConnectorCredential
            {
                ConnectorName = this.ConnectorName,
                ConnectorId = this.ConnectorKey,
                ConnectorProperties = new PropertyCollection<string>(this.ConnectorProperties)
            };
        }
    }
}
