// <copyright file="Record.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract
{
    [DataContract]
    public class Record<TMessage>
    {
        public Record()
        {
        }

        public Record(TMessage item, RecordInfo recordInfo)
        {
            this.Item = item;
            this.RecordInfo = recordInfo;
        }

        [DataMember]
        public TMessage Item { get; private set; }

        [DataMember]
        public RecordInfo RecordInfo { get; private set; }
    }
}
