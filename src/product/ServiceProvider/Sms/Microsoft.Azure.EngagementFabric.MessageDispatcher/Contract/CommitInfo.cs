// <copyright file="CommitInfo.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Contract
{
    public class CommitInfo
    {
        public RecordInfo RecordInfo { get; set; }

        public bool IsCommited { get; set; }

        public DateTime AppendTime { get; set; }
    }
}
