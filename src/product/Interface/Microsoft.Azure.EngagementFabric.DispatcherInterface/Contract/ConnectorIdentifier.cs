// <copyright file="ConnectorIdentifier.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    /// <summary>
    /// Identifier of a connector, consists of name and Id
    /// </summary>
    [DataContract]
    public class ConnectorIdentifier : IEquatable<ConnectorIdentifier>
    {
        public ConnectorIdentifier()
        {
        }

        public ConnectorIdentifier(string name, string id)
        {
            this.ConnectorName = name;
            this.ConnectorId = id;
        }

        public ConnectorIdentifier(ConnectorIdentifier other)
        {
            this.ConnectorName = other.ConnectorName;
            this.ConnectorId = other.ConnectorId;
        }

        [DataMember]
        public string ConnectorName { get; set; }

        [DataMember]
        public string ConnectorId { get; set; }

        public bool Equals(ConnectorIdentifier other)
        {
            return other != null &&
                string.Equals(this.ConnectorName, other.ConnectorName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.ConnectorId, other.ConnectorId, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"name={this.ConnectorName} id={this.ConnectorId}";
        }

        public override int GetHashCode()
        {
            return string.Concat(this.ConnectorName, this.ConnectorId).GetHashCode();
        }
    }
}
