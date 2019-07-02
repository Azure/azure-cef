// <copyright file="OutputMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    [DataContract]
    public class OutputMessage : IEquatable<OutputMessage>
    {
        public OutputMessage()
        {
            this.Id = Guid.NewGuid();
        }

        public OutputMessage(InputMessage inputMessage, ReadOnlyCollection<string> targets, OutputMessageState state)
            : this()
        {
            this.MessageInfo = inputMessage.MessageInfo;
            this.Targets = targets;
            this.ConnectorCredential = inputMessage.ConnectorCredential;
            this.ReportingServiceUri = inputMessage.ReportingServiceUri;
            this.State = state;
        }

        [DataMember]
        public Guid Id { get; set; }

        [DataMember]
        public MessageInfo MessageInfo { get; set; }

        [DataMember]
        public ReadOnlyCollection<string> Targets { get; set; }

        [DataMember]
        public ConnectorCredential ConnectorCredential { get; set; }

        [DataMember]
        public string ReportingServiceUri { get; set; }

        [DataMember]
        public bool Delivered { get; set; }

        // Properties internal used only by dispatcher service
        [DataMember]
        public OutputMessageState State { get; set; }

        [DataMember]
        public DateTime DeliveryTime { get; set; }

        [DataMember]
        public int DeliveryCount { get; set; }

        // Used within a round of dispatch. No need to serialize
        public DateTime RequestExpiration { get; set; }

        public bool Equals(OutputMessage other)
        {
            return other != null && other.Id != Guid.Empty && other.Id == Id;
        }
    }
}
