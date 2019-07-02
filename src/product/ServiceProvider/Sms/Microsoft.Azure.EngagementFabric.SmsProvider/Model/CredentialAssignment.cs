// <copyright file="CredentialAssignment.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class CredentialAssignment
    {
        public CredentialAssignment()
        {
        }

        public CredentialAssignment(ConnectorCredentialAssignment assignment)
        {
            this.ChannelType = assignment.ChannelType;
            this.Provider = assignment.ConnectorIdentifier.ConnectorName;
            this.ConnectorId = assignment.ConnectorIdentifier.ConnectorId;
            this.Enabled = assignment.Enabled;
            this.Active = assignment.Active;
            this.ExtendedCode = assignment.ExtendedCode;
        }

        [JsonProperty(PropertyName = "ChannelType", Required = Required.Always)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ChannelType ChannelType { get; set; }

        [JsonProperty(PropertyName = "Provider", Required = Required.Always)]
        public string Provider { get; set; }

        [JsonProperty(PropertyName = "Id", Required = Required.Always)]
        public string ConnectorId { get; set; }

        [JsonProperty(PropertyName = "Enabled", Required = Required.Always)]
        public bool Enabled { get; set; }

        [JsonProperty(PropertyName = "Active", Required = Required.Always)]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "ExtendedCode", NullValueHandling = NullValueHandling.Ignore)]
        public string ExtendedCode { get; set; }

        public ConnectorCredentialAssignment ToDataContract(string account)
        {
            return new ConnectorCredentialAssignment
            {
                EngagementAccount = account,
                ChannelType = this.ChannelType,
                ConnectorIdentifier = new ConnectorIdentifier(this.Provider, this.ConnectorId),
                Enabled = this.Enabled,
                Active = this.Active,
                ExtendedCode = this.ExtendedCode
            };
        }
    }
}
