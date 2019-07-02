// <copyright file="ReliableLog.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Common.Threading;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Exceptions;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Storage
{
    public class ReliableLog<TMessage> : RunAsyncComponent, IReliableLog<TMessage>
    {
        private static readonly TimeSpan MaxDelayBeforeTransactionStuck = TimeSpan.FromMinutes(10);

        private readonly DispatcherQueueSetting setting;
        private IReliableStateManager stateManager;
        private long maxQueueLength;

        // Data store
        private IReliableDictionary<long, Record<TMessage>> recordDictionary;
        private string recordDictionaryKeyName;

        // Metadata of log, e.g. checkpoint, last committed sequence number
        private IReliableDictionary<string, RecordInfo> metaDictonary;
        private string metaDictionaryKeyName;
        private string lastCommittedIndexKeyName;
        private string checkpointKeyName;

        // In memory variables
        private RecordInfo lastIndex;
        private RecordInfo lastCommittedIndex;
        private RecordInfo currentCheckpointIndex;
        private AsyncWaiter waiter;
        private List<CommitInfo> inflightAppends;

        public ReliableLog(
            DispatcherQueueSetting setting,
            IReliableStateManager stateManager)
            : base(nameof(ReliableLog<TMessage>))
        {
            this.setting = setting;
            this.stateManager = stateManager;
            this.maxQueueLength = setting.MaxQueueLength;
            this.recordDictionaryKeyName = this.setting.Name + ":RecordDictionary";
            this.metaDictionaryKeyName = this.setting.Name + ":MetaDictionary";
            this.lastCommittedIndexKeyName = this.setting.Name + ":LastCommit";
            this.checkpointKeyName = this.setting.Name + ":Checkpoint";

            this.lastIndex = this.lastCommittedIndex = this.currentCheckpointIndex = new RecordInfo(RecordInfo.InvalidIndex, this.maxQueueLength);
            this.inflightAppends = new List<CommitInfo>();
            this.waiter = new AsyncWaiter();
        }

        public DispatcherQueueSetting Setting => this.setting;

        public long Length
        {
            get
            {
                if (this.lastCommittedIndex.Index == RecordInfo.InvalidIndex)
                {
                    return 0;
                }
                else if (this.currentCheckpointIndex.Index == RecordInfo.InvalidIndex)
                {
                    return this.lastCommittedIndex.Index + 1;
                }
                else if (this.lastCommittedIndex.Index >= this.currentCheckpointIndex.Index)
                {
                    return this.lastCommittedIndex.Index - currentCheckpointIndex.Index;
                }
                else
                {
                    return this.lastCommittedIndex.Index + this.setting.MaxQueueLength - currentCheckpointIndex.Index;
                }
            }
        }

        public int InflightAppendTaskCount => this.inflightAppends.Count;

        public static IReliableLog<TMessage> Create(DispatcherQueueSetting setting, IReliableStateManager stateManager)
        {
            return new ReliableLog<TMessage>(setting, stateManager);
        }

        public override string ToString()
        {
            return TracingHelper.FormatTraceSource(this, this.setting.Name);
        }

        public override string GetTraceState()
        {
            return $"Component={this.Component} Queue={this.setting.Name} Length={this.Length} InflightAppendTaskCount={this.InflightAppendTaskCount}";
        }

        public async Task AppendAsync(IReadOnlyList<TMessage> messages)
        {
            if (messages == null || messages.Count <= 0)
            {
                return;
            }

            var startIndex = PrepareAppend(messages.Count);
            try
            {
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.AppendAsync), OperationStates.Starting, $"AppendStarting from Index={startIndex.Index}.");

                var index = new RecordInfo(startIndex);
                using (var tx = this.stateManager.CreateTransaction())
                {
                    for (var i = 0; i < messages.Count; i++)
                    {
                        var record = new Record<TMessage>(messages[i], index);
                        await this.recordDictionary.AddOrUpdateAsync(tx, index.Index, record, (key, value) =>
                        {
                            if (value != null)
                            {
                                throw new QueueSizeExceededException(this.Setting.Name);
                            }
                            else
                            {
                                return record;
                            }
                        });

                        if (i != messages.Count - 1)
                        {
                            index = index.Next();
                        }
                    }

                    await tx.CommitAsync();
                }

                // Commit if no exception
                await CompleteAppendAsync(startIndex, messages.Count, false);
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.AppendAsync), OperationStates.Succeeded, $"AppendCompleted to LastIndex={index.Index}, Count={messages.Count}.");
            }
            catch (Exception ex)
            {
                await CompleteAppendAsync(startIndex, messages.Count, true);
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.AppendAsync), OperationStates.Failed, string.Empty, ex);
            }
        }

        public async Task CheckpointAsync(CheckpointInfo checkpointInfo)
        {
            if (checkpointInfo.RecordCount <= 0)
            {
                return;
            }

            var index = checkpointInfo.FirstRecordInfo;
            var counting = 0;
            using (var tx = this.stateManager.CreateTransaction())
            {
                // Two break conditions:
                // 1. counting == checkpointInfo.RecordCount
                // 2. index == checkpointInfo.LastRecordInfo
                // After breaking, double check if both condition is true.
                // Otherwise there's a mismatch within checkpointInfo
                while (counting < checkpointInfo.RecordCount)
                {
                    var removed = await recordDictionary.TryRemoveAsync(tx, index.Index);
                    if (removed.HasValue && removed.Value != null)
                    {
                        counting++;
                    }

                    if (index.Equals(checkpointInfo.LastRecordInfo))
                    {
                        break;
                    }

                    index = index.Next();
                }

                if (!index.Equals(checkpointInfo.LastRecordInfo) || counting != checkpointInfo.RecordCount)
                {
                    MessageDispatcherEventSource.Current.Warning(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CheckpointAsync), OperationStates.NoMatch, $"Mismatch in checkpoint info. LastRecordInfo={checkpointInfo.LastRecordInfo.Index}, Index={index.Index}, RecordCount={checkpointInfo.RecordCount}, Counting={counting}");
                }

                await this.metaDictonary.AddOrUpdateAsync(tx, this.checkpointKeyName, checkpointInfo.LastRecordInfo, (k, v) => checkpointInfo.LastRecordInfo);
                await tx.CommitAsync();
            }

            lock (this.Lock)
            {
                this.currentCheckpointIndex = checkpointInfo.LastRecordInfo;
            }

            MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CheckpointAsync), OperationStates.Succeeded, $"Checkpoint CurrentCheckpoint={checkpointInfo.LastRecordInfo}. NumberOfRecords={checkpointInfo.RecordCount}, Actual NumberOfRecords={counting}");
        }

        public async Task<RecordInfo> GetCheckpointedRecordInfoAsync()
        {
            using (var tx = this.stateManager.CreateTransaction())
            {
                var value = await this.metaDictonary.TryGetValueAsync(tx, this.checkpointKeyName);
                if (value.HasValue)
                {
                    return value.Value;
                }
            }

            return new RecordInfo(RecordInfo.InvalidIndex, this.maxQueueLength);
        }

        public async Task<IReadOnlyList<Record<TMessage>>> ReadAsync(RecordInfo recordInfo, int numberOfRecords, bool inclusive, TimeSpan timeout, CancellationToken cancellationToken)
        {
            bool foundAny = await this.WaitForMessagesAsync(recordInfo, inclusive, timeout, cancellationToken);
            if (!foundAny)
            {
                return null;
            }

            var recordList = new List<Record<TMessage>>();
            try
            {
                var index = recordInfo.Index == RecordInfo.InvalidIndex ? recordInfo.Next() : recordInfo;

                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.ReadAsync), OperationStates.Starting, $"RecordRead starting at Index={index.Index}, NumberOfRecords={numberOfRecords}, Inclusive={inclusive}");

                while (recordList.Count < numberOfRecords)
                {
                    using (var transaction = this.stateManager.CreateTransaction())
                    {
                        if (inclusive)
                        {
                            var result = await this.recordDictionary.TryGetValueAsync(transaction, index.Index);
                            if (result.HasValue && result.Value != null)
                            {
                                recordList.Add(result.Value);
                            }
                        }
                        else
                        {
                            inclusive = true;
                        }

                        if (index.Equals(this.lastCommittedIndex))
                        {
                            break;
                        }

                        index = index.Next();
                    }
                }

                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.ReadAsync), OperationStates.Succeeded, $"RecordRead completed with NumberOfRecords={recordList.Count}");

                return recordList;
            }
            catch (Exception ex)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.ReadAsync), OperationStates.Failed, string.Empty, ex);
                throw;
            }
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            // Recover record dictionary
            this.recordDictionary = await stateManager.GetOrAddAsync<IReliableDictionary<long, Record<TMessage>>>(recordDictionaryKeyName);

            // Recover meta dictionary
            this.metaDictonary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, RecordInfo>>(this.metaDictionaryKeyName);
            this.currentCheckpointIndex = await GetCheckpointedRecordInfoAsync();

            using (var tx = this.stateManager.CreateTransaction())
            {
                var value = await this.metaDictonary.TryGetValueAsync(tx, this.lastCommittedIndexKeyName);
                if (value.HasValue)
                {
                    this.lastCommittedIndex = this.lastIndex = value.Value;
                }
            }
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            this.waiter.Close();
            await base.OnCloseAsync(cancellationToken);
        }

        private RecordInfo PrepareAppend(int messageCount)
        {
            lock (this.Lock)
            {
                var startIndex = this.lastIndex.Next();
                for (var i = 0; i < messageCount; i++)
                {
                    this.inflightAppends.Add(new CommitInfo
                    {
                        RecordInfo = new RecordInfo(this.lastIndex.Next()),
                        IsCommited = false,
                        AppendTime = DateTime.UtcNow
                    });

                    this.lastIndex = this.lastIndex.Next();
                }

                return startIndex;
            }
        }

        private async Task CompleteAppendAsync(RecordInfo startRecordInfo, int messageCount, bool appendFailed)
        {
            if (startRecordInfo == null || startRecordInfo.Index == RecordInfo.InvalidIndex || messageCount <= 0)
            {
                return;
            }

            lock (this.Lock)
            {
                for (var i = 0; i < messageCount; i++)
                {
                    var recordInfo = new RecordInfo(startRecordInfo.Index + i, startRecordInfo.MaxQueueLength);
                    var find = this.inflightAppends.SingleOrDefault(r => r.RecordInfo.Equals(recordInfo));
                    if (find == null)
                    {
                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CompleteAppendAsync), OperationStates.Empty, $"Record not found in InFlightAppends [Index={recordInfo.Index}]");
                        continue;
                    }

                    if (appendFailed)
                    {
                        inflightAppends.Remove(find);
                    }
                    else
                    {
                        find.IsCommited = true;
                    }
                }

                RecordInfo highestCommittedIndex = null;
                int committedMessageCount = 0;
                foreach (var commitInfo in this.inflightAppends)
                {
                    if (commitInfo.IsCommited)
                    {
                        highestCommittedIndex = commitInfo.RecordInfo;
                        committedMessageCount++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (committedMessageCount > 0)
                {
                    inflightAppends.RemoveRange(0, committedMessageCount);
                    this.lastCommittedIndex = highestCommittedIndex;
                    this.waiter.Set();
                }
            }

            if (!appendFailed)
            {
                using (var tx = this.stateManager.CreateTransaction())
                {
                    await this.metaDictonary.AddOrUpdateAsync(tx, this.lastCommittedIndexKeyName, this.lastCommittedIndex, (k, v) => this.lastCommittedIndex, TimeSpan.FromSeconds(30), CancellationToken.None);
                    await tx.CommitAsync();
                }
            }
        }

        private async Task<bool> WaitForMessagesAsync(RecordInfo recordInfo, bool inclusive, TimeSpan waitTimeout, CancellationToken cancelToken)
        {
            bool wait = false;

            lock (this.Lock)
            {
                if (this.lastCommittedIndex.Index == RecordInfo.InvalidIndex)
                {
                    // No message committed
                    wait = true;
                }
                else if (inclusive)
                {
                    // There's at least one data and the read is inclusive
                    wait = false;
                }
                else if (recordInfo.Equals(this.lastCommittedIndex))
                {
                    // Request record is the last data and the read is not inclusive
                    wait = true;
                }
            }

            if (wait)
            {
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.WaitForMessagesAsync), OperationStates.Starting, string.Empty);

                try
                {
                    lock (this.Lock)
                    {
                        this.waiter.Initialize();
                    }

                    if (!await this.waiter.WaitAsync(waitTimeout, cancelToken))
                    {
                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.WaitForMessagesAsync), OperationStates.TimedOut, string.Empty);
                        return false;
                    }
                    else if (cancelToken.IsCancellationRequested)
                    {
                        MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.WaitForMessagesAsync), OperationStates.Skipping, "Operation Cancelled");
                        return false;
                    }
                }
                finally
                {
                    this.waiter.Reset();
                }
            }

            return true;
        }
    }
}
