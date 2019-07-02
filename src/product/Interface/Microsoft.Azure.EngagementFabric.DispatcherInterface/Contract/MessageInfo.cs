// <copyright file="MessageInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    /// <summary>
    /// Dispatched message info, including message details and metadata
    /// </summary>
    [DataContract]
    public class MessageInfo
    {
        [DataMember]
        public string EngagementAccount { get; set; }

        [DataMember]
        public Guid MessageId { get; set; }

        [DataMember]
        public string TrackingId { get; set; }

        [DataMember]
        public DateTime SendTime { get; set; }

        /// <summary>
        /// Gets or sets MessageBody
        /// Depends on derived class, the MessageBody can be plain text for structured serialized object
        /// </summary>
        [DataMember]
        public string MessageBody { get; set; }

        /// <summary>
        /// Gets or sets ExtensionData
        /// ExtensionData is for derived class to store extension information
        /// Derived class is responsible to serialize and de-serialize
        /// </summary>
        [DataMember]
        public string ExtensionData { get; set; }
    }
}
