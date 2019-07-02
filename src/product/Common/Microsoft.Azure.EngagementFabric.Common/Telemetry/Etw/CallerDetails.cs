// -----------------------------------------------------------------------
// <copyright file="CallerDetails.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    public struct CallerDetails
    {
        public static readonly CallerDetails Empty = new CallerDetails(string.Empty, string.Empty);

        public CallerDetails(string callerId, string callerState)
        {
            this.Id = callerId;
            this.State = callerState;
        }

        public string Id { get; }

        public string State { get; }
    }
}
