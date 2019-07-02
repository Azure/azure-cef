// <copyright file="IResultReporter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.ObjectModel;
using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.MessageDispatcher.Common;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Dispatcher
{
    public interface IResultReporter : IComponent
    {
        void ReporAndForgetAsync(string reportingServiceUri, ReadOnlyCollection<OutputResult> results);
    }
}
