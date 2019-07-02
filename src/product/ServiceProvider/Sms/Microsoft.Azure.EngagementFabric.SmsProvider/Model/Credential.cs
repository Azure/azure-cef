// <copyright file="Credential.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.Common.Collection;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class Credential
    {
        public Credential()
        {
        }

        public Credential(SmsConnectorCredential connectorCredential)
        {
            this.ConnectorName = connectorCredential.ConnectorName;
            this.ConnectorKey = connectorCredential.ConnectorId;
            this.ChannelType = connectorCredential.ChannelType;
            this.ConnectorProperties = new PropertyCollection<string>(connectorCredential.ConnectorProperties);
        }

        [JsonProperty(PropertyName = "Provider", Required = Required.Always)]
        public string ConnectorName { get; set; }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public string ConnectorKey { get; set; }

        [JsonProperty(PropertyName = "ChannelType", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ChannelType ChannelType { get; set; }

        [JsonProperty(PropertyName = "Properties", Required = Required.Always)]
        public PropertyCollection<string> ConnectorProperties { get; set; }

        public SmsConnectorCredential ToConnectorCredential()
        {
            return new SmsConnectorCredential
            {
                ConnectorName = this.ConnectorName,
                ConnectorId = this.ConnectorKey,
                ChannelType = this.ChannelType,
                ConnectorProperties = new PropertyCollection<string>(this.ConnectorProperties)
            };
        }
    }
}
