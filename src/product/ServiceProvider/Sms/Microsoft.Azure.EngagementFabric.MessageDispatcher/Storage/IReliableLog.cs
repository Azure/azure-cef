// <copyright file="IReliableLog.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Storage
{
    public interface IReliableLog<TMessage> : IComponent
    {
        long Length { get; }

        DispatcherQueueSetting Setting { get; }

        int InflightAppendTaskCount { get; }

        Task AppendAsync(IReadOnlyList<TMessage> messages);

        Task<IReadOnlyList<Record<TMessage>>> ReadAsync(RecordInfo recordInfo, int numberOfRecords, bool inclusive, TimeSpan timeout, CancellationToken cancellationToken);

        Task CheckpointAsync(CheckpointInfo checkpointInfo);

        Task<RecordInfo> GetCheckpointedRecordInfoAsync();
    }
}
