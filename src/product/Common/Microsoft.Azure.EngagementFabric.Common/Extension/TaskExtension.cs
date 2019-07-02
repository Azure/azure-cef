// <copyright file="TaskExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.Common.Extension
{
    public static class TaskExtension
    {
        public static void Fork(this Task task)
        {
            task.ContinueWith(t => { });
        }
    }
}
