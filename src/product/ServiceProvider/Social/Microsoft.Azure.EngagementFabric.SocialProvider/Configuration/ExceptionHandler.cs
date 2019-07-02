// <copyright file="ExceptionHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Configuration
{
    public static class ExceptionHandler
    {
        // There are three kinds of exception:
        // 1. user generated (e.g. bad request)
        // 2. service internal error
        // 3. external dependency error (e.g. from NH)
        // For case#1, it is not an issue so that we don't want to monitor in prod env
        public static void ProcessException(object caller, Exception ex, string trackingId = null)
        {
            if (string.IsNullOrEmpty(trackingId))
            {
                trackingId = SocialProviderEventSource.EmptyTrackingId;
            }

#if DEBUG
            SocialProviderEventSource.Current.ErrorException(trackingId, caller, nameof(ExceptionHandler), OperationStates.Failed, string.Empty, ex);
#else
            if (!(ex is ArgumentException) &&
                !(ex is ArgumentNullException) &&
                !(ex is ArgumentOutOfRangeException) &&
                !(ex is ResourceNotFoundException))
            {
                SocialProviderEventSource.Current.ErrorException(trackingId, caller, nameof(ExceptionHandler), OperationStates.Failed, string.Empty, ex);
            }
#endif
        }
    }
}
