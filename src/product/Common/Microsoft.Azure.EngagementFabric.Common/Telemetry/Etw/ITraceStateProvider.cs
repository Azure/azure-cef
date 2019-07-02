// -----------------------------------------------------------------------
// <copyright file="ITraceStateProvider.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    public interface ITraceStateProvider
    {
        string GetTraceState();
    }
}
