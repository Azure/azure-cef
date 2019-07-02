// <copyright file="SmsConnectorCredential.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Credential
{
    [DataContract]
    public class SmsConnectorCredential : ConnectorIdentifier
    {
        [DataMember]
        public ChannelType ChannelType { get; set; }

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
