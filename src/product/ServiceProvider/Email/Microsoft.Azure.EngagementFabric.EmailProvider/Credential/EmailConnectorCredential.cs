// <copyright file="EmailConnectorCredential.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Credential
{
    [DataContract]
    public class EmailConnectorCredential : ConnectorIdentifier
    {
        [DataMember]
        public PropertyCollection<string> ConnectorProperties { get; set; }

        public ConnectorCredential ToDataContract(ConnectorMetadata metadata)
        {
            var credential = new ConnectorCredential();
            credential.ConnectorName = this.ConnectorName;
            credential.ConnectorId = this.ConnectorId;
            credential.BatchSize = metadata.BatchSize;
            credential.ConnectorUri = metadata.ConnectorUri;
            credential.ConnectorProperties = new PropertyCollection<string>(this.ConnectorProperties);

            return credential;
        }
    }
}
