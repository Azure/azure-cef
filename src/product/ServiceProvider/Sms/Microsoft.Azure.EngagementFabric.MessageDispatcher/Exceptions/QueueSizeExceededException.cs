// <copyright file="QueueSizeExceededException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Exceptions
{
    public class QueueSizeExceededException : Exception
    {
        public QueueSizeExceededException(string name)
            : base($"Queue {name} exceeded the size limit")
        {
        }
    }
}
