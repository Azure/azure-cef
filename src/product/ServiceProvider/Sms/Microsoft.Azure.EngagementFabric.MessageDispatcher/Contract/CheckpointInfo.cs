// <copyright file="CheckpointInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract
{
    public class CheckpointInfo
    {
        public CheckpointInfo(RecordInfo firstRecordInfo, RecordInfo lastRecordInfo, int recordCount)
        {
            this.FirstRecordInfo = firstRecordInfo;
            this.LastRecordInfo = lastRecordInfo;
            this.RecordCount = recordCount;
            this.Tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public RecordInfo FirstRecordInfo { get; set; }

        public RecordInfo LastRecordInfo { get; set; }

        public int RecordCount { get; set; }

        public int Retries { get; set; }

        public TaskCompletionSource<bool> Tcs { get; }

        public override string ToString()
        {
            return $"FirstRecord={this.FirstRecordInfo}, LastRecord={this.LastRecordInfo}, RecordCount={this.RecordCount}, Retries={this.Retries}";
        }
    }
}
