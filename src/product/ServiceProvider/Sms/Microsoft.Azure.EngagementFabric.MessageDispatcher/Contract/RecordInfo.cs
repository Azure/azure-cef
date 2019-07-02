// <copyright file="RecordInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract
{
    [DataContract]
    public class RecordInfo : IEquatable<RecordInfo>
    {
        public const long InvalidIndex = -1;

        public RecordInfo(long index, long maxQueueLength)
        {
            this.Index = maxQueueLength > 0 && index >= 0 ? index % maxQueueLength : RecordInfo.InvalidIndex;
            this.MaxQueueLength = maxQueueLength;
        }

        public RecordInfo(RecordInfo other)
        {
            this.Index = other.Index;
            this.MaxQueueLength = other.MaxQueueLength;
        }

        [DataMember]
        public long Index { get; private set; }

        [DataMember]
        public long MaxQueueLength { get; private set; }

        public RecordInfo Next(int count = 1)
        {
            return new RecordInfo(this.Index + count, this.MaxQueueLength);
        }

        public bool Equals(RecordInfo other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Index == other.Index && this.MaxQueueLength == other.MaxQueueLength;
        }

        public override string ToString()
        {
            return $"Index={this.Index}";
        }
    }
}
