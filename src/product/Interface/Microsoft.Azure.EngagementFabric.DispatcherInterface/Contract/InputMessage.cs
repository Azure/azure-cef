// <copyright file="InputMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    /// <summary>
    /// Message received by dispatcher service
    /// </summary>
    [DataContract]
    public class InputMessage : IEquatable<InputMessage>
    {
        public InputMessage()
        {
        }

        // This is for re-filtering
        public InputMessage(OutputMessage outputMessage)
        {
            this.MessageInfo = outputMessage.MessageInfo;
            this.Targets = outputMessage.Targets;
            this.ConnectorCredential = outputMessage.ConnectorCredential;
            this.ReportingServiceUri = outputMessage.ReportingServiceUri;
        }

        [DataMember]
        public MessageInfo MessageInfo { get; set; }

        [DataMember]
        public ReadOnlyCollection<string> Targets { get; set; }

        [DataMember]
        public ConnectorCredential ConnectorCredential { get; set; }

        [DataMember]
        public string ReportingServiceUri { get; set; }

        public bool Equals(InputMessage other)
        {
            return this.MessageInfo != null &&
                other?.MessageInfo != null &&
                other.MessageInfo.MessageId != Guid.Empty &&
                other.MessageInfo.MessageId == this.MessageInfo.MessageId;
        }
    }
}
