// -----------------------------------------------------------------------
// <copyright file="EventSourceBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using static Microsoft.Azure.EngagementFabric.Common.Telemetry.EventSourceHelper;

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Allow EventId constants to be defined next to the associated event method")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:CommasMustBeSpacedCorrectly", Justification = "Allow EventAttribute with Message conditionally compiled")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "Allow EventAttribute with Message conditionally compiled")]
    public abstract class EventSourceBase : EventSource
    {
        public static readonly string EmptyTrackingId = string.Empty;

        [NonEvent]
        public void Verbose(string trackingId, object source, string operation, string operationState, string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (this.IsEnabled())
            {
                var callerDetails = GetCallerDetails(source);
                this.WriteVerbose(trackingId ?? EmptyTrackingId, callerDetails.Id, callerDetails.State, operation, operationState, message, line, file);
            }
        }

        [NonEvent]
        public void Info(string trackingId, object source, string operation, string operationState, string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (this.IsEnabled())
            {
                var callerDetails = GetCallerDetails(source);
                this.WriteInfo(trackingId ?? EmptyTrackingId, callerDetails.Id, callerDetails.State, operation, operationState, message, line, file);
            }
        }

        [NonEvent]
        public void Warning(string trackingId, object source, string operation, string operationState, string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (this.IsEnabled())
            {
                var callerDetails = GetCallerDetails(source);
                this.WriteWarning(trackingId ?? EmptyTrackingId, callerDetails.Id, callerDetails.State, operation, operationState, message, line, file);
            }
        }

        [NonEvent]
        public void Error(string trackingId, object source, string operation, string operationState, string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (this.IsEnabled())
            {
                var callerDetails = GetCallerDetails(source);
                this.WriteError(trackingId ?? EmptyTrackingId, callerDetails.Id, callerDetails.State, operation, operationState, message, line, file);
            }
        }

        [NonEvent]
        public void Critical(string trackingId, object source, string operation, string operationState, string message, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (this.IsEnabled())
            {
                var callerDetails = GetCallerDetails(source);
                this.WriteCritical(trackingId ?? EmptyTrackingId, callerDetails.Id, callerDetails.State, operation, operationState, message, line, file);
            }
        }

        [NonEvent]
        public void ErrorException(string trackingId, object source, string operation, string operationState, string message, Exception exception, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (this.IsEnabled())
            {
                var callerDetails = GetCallerDetails(source);
                this.WriteError(trackingId ?? EmptyTrackingId, callerDetails.Id, callerDetails.State, operation, operationState, FormatMessageAndException(message, exception), line, file);
            }
        }

        [NonEvent]
        public void CriticalException(string trackingId, object source, string operation, string operationState, string message, Exception exception, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (this.IsEnabled())
            {
                var callerDetails = GetCallerDetails(source);
                this.WriteCritical(trackingId ?? EmptyTrackingId, callerDetails.Id, callerDetails.State, operation, operationState, FormatMessageAndException(message, exception), line, file);
            }
        }

        [NonEvent]
        public void Unexpected(string trackingId, object source, string operation, string operationState, string msg, [CallerLineNumber] int line = 0, [CallerFilePath] string file = "")
        {
            if (this.IsEnabled())
            {
                var callerDetails = GetCallerDetails(source);
                this.EventTrackingUnexpected(trackingId ?? EmptyTrackingId, callerDetails.Id, callerDetails.State, operation, operationState, msg, line, file);
            }
        }

        protected abstract void WriteVerbose(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file);

        protected abstract void WriteInfo(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file);

        protected abstract void WriteWarning(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file);

        protected abstract void WriteError(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file);

        protected abstract void WriteCritical(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file);

        protected abstract void WriteErrorException(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file);

        protected abstract void WriteCriticalException(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file);

        protected abstract void EventTrackingUnexpected(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file);
    }
}