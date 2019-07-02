// <copyright file="DelayedReliableLog.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Storage
{
    public class DelayedReliableLog : BaseComponent, IReliableLog<OutputMessage>
    {
        private readonly IReliableLog<OutputMessage> reliableLog;
        private IReadOnlyList<Record<OutputMessage>> remainingRecords;
        private RecordInfo lastRecord;

        public DelayedReliableLog(IReliableLog<OutputMessage> reliableLog)
            : base(nameof(DelayedReliableLog))
        {
            this.reliableLog = reliableLog;
        }

        public long Length => this.reliableLog.Length;

        public DispatcherQueueSetting Setting => this.reliableLog.Setting;

        public int InflightAppendTaskCount => this.reliableLog.InflightAppendTaskCount;

        public override string GetTraceState()
        {
            return $"Component={this.Component} Queue={this.Setting.Name} Length={this.Length} InflightAppendTaskCount={this.InflightAppendTaskCount}";
        }

        public Task AppendAsync(IReadOnlyList<OutputMessage> messages)
        {
            return this.reliableLog.AppendAsync(messages);
        }

        public Task CheckpointAsync(CheckpointInfo checkpointInfo)
        {
            return this.reliableLog.CheckpointAsync(checkpointInfo);
        }

        public Task<RecordInfo> GetCheckpointedRecordInfoAsync()
        {
            return this.reliableLog.GetCheckpointedRecordInfoAsync();
        }

        public async Task<IReadOnlyList<Record<OutputMessage>>> ReadAsync(RecordInfo recordInfo, int numberOfRecords, bool inclusive, TimeSpan timeout, CancellationToken cancellationToken)
        {
            IReadOnlyList<Record<OutputMessage>> readRecords = null;
            if (this.remainingRecords != null && !inclusive && this.lastRecord != null && this.lastRecord.Equals(recordInfo))
            {
                readRecords = this.remainingRecords;
                this.remainingRecords = null;
                this.lastRecord = null;
            }
            else
            {
                this.remainingRecords = null;
                this.lastRecord = null;
                readRecords = await this.reliableLog.ReadAsync(recordInfo, numberOfRecords, inclusive, timeout, cancellationToken);
            }

            if (readRecords != null && readRecords.Count > 0)
            {
                var currentRecord = readRecords[0];
                var delay = currentRecord.Item.DeliveryTime - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                {
                    if (timeout < delay)
                    {
                        await Task.Delay(timeout, cancellationToken);
                        return null;
                    }

                    await Task.Delay(delay, cancellationToken);
                }

                var now = DateTime.UtcNow;
                var index = 1;
                while (index < readRecords.Count)
                {
                    if (readRecords[index].Item.DeliveryTime > now)
                    {
                        break;
                    }

                    index++;
                }

                if (index < readRecords.Count)
                {
                    this.remainingRecords = readRecords.ToList().GetRange(index, readRecords.Count - index);
                    this.lastRecord = readRecords[index - 1].RecordInfo;
                }

                return readRecords.ToList().GetRange(0, index);
            }

            return readRecords;
        }

        protected override Task OnOpenAsync(CancellationToken cancellationToken)
        {
            return this.reliableLog.OpenAsync(cancellationToken);
        }

        protected override Task OnCloseAsync(CancellationToken cancellationToken)
        {
            return this.reliableLog.CloseAsync(cancellationToken);
        }
    }
}
