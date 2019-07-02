// <copyright file="MessageIdentifer.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class MessageIdentifer
    {
        public MessageIdentifer()
        {
        }

        public MessageIdentifer(string messageId, string customMessageId)
        {
            this.MessageId = messageId;
            this.CustomMessageId = customMessageId;
        }

        [DataMember]
        public string MessageId { get; set; }

        [DataMember]
        public string CustomMessageId { get; set; }
    }
}
