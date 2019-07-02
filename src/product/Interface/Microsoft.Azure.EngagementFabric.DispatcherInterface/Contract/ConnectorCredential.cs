// <copyright file="ConnectorCredential.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Collection;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    /// <summary>
    /// Connector details, including all informations of connection to the provider
    /// </summary>
    [DataContract]
    public class ConnectorCredential : ConnectorIdentifier
    {
        public ConnectorCredential()
        {
        }

        public ConnectorCredential(ConnectorCredential other)
            : base(other)
        {
            this.BatchSize = other.BatchSize;
            this.ConnectorUri = other.ConnectorUri;
            this.ConnectorProperties = new PropertyCollection<string>(other.ConnectorProperties);
        }

        [DataMember]
        public long BatchSize { get; set; }

        [DataMember]
        public string ConnectorUri { get; set; }

        [DataMember]
        public PropertyCollection<string> ConnectorProperties { get; set; }

        public override string ToString()
        {
            return $"name={this.ConnectorName} id={this.ConnectorId} uri={this.ConnectorUri} batchsize={this.BatchSize}";
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.ConnectorName) &&
                !string.IsNullOrEmpty(this.ConnectorId) &&
                !string.IsNullOrEmpty(this.ConnectorUri) &&
                BatchSize > 0;
        }
    }
}
