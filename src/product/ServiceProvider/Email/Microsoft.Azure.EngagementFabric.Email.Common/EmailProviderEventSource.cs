// <copyright file="EmailProviderEventSource.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using static Microsoft.Azure.EngagementFabric.Common.Telemetry.EventSourceHelper;

namespace Microsoft.Azure.EngagementFabric.Email.Common
{
    [EventSource(Name = "Microsoft-Azure-EngagementFabric-Email")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Allow EventId constants to be defined next to the associated event method")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsMustBeSeparatedByBlankLine", Justification = "Allow EventId constants to be defined next to the associated event method")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:CommasMustBeSpacedCorrectly", Justification = "Allow EventAttribute with Message conditionally compiled")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "Allow EventAttribute with Message conditionally compiled")]
    public sealed class EmailProviderEventSource : EventSourceBase
    {
        public static readonly EmailProviderEventSource Current = new EmailProviderEventSource();

        static EmailProviderEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        // Instance constructor is private to enforce singleton semantics
        private EmailProviderEventSource()
            : base()
        {
        }

        #region Keywords

        // Event keywords can be used to categorize events.
        // Each keyword is a bit flag. A single event can be associated with multiple keywords (via EventAttribute.Keywords property).
        // Keywords must be defined as a public class named 'Keywords' inside EventSource that uses them.
        public static class Keywords
        {
            public const EventKeywords Debug = (EventKeywords)0x1L;
        }
        #endregion

        private const int VerboseEventId = 101;
        private const int InfoEventId = 102;
        private const int WarningEventId = 103;
        private const int ErrorEventId = 104;
        private const int CriticalEventId = 105;
        private const int ErrorExceptionEventId = 106;
        private const int CriticalExceptionEventId = 107;
        private const int UnexpectedEventId = 108;

        [Event(VerboseEventId, Version = EventVersion, Level = EventLevel.Verbose, Keywords = Keywords.Debug)]
        protected override void WriteVerbose(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file)
        {
            this.WriteEvent(VerboseEventId, trackingId, callerId, callerState, operation, operationState, msg, line, GetFileName(file));
        }

        [Event(InfoEventId, Version = EventVersion, Level = EventLevel.Informational, Keywords = Keywords.Debug)]
        protected override void WriteInfo(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file)
        {
            this.WriteEvent(InfoEventId, trackingId, callerId, callerState, operation, operationState, msg, line, GetFileName(file));
        }

        [Event(WarningEventId, Version = EventVersion, Level = EventLevel.Warning, Keywords = Keywords.Debug)]
        protected override void WriteWarning(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file)
        {
            this.WriteEvent(WarningEventId, trackingId, callerId, callerState, operation, operationState, msg, line, GetFileName(file));
        }

        [Event(ErrorEventId, Version = EventVersion, Level = EventLevel.Error, Keywords = Keywords.Debug)]
        protected override void WriteError(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file)
        {
            this.WriteEvent(ErrorEventId, trackingId, callerId, callerState, operation, operationState, msg, line, GetFileName(file));
        }

        [Event(CriticalEventId, Version = EventVersion, Level = EventLevel.Error, Keywords = Keywords.Debug)]
        protected override void WriteCritical(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file)
        {
            this.WriteEvent(CriticalEventId, trackingId, callerId, callerState, operation, operationState, msg, line, GetFileName(file));
        }

        [Event(ErrorExceptionEventId, Version = EventVersion, Level = EventLevel.Error, Keywords = Keywords.Debug)]
        protected override void WriteErrorException(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file)
        {
            this.WriteEvent(ErrorEventId, trackingId, callerId, callerState, operation, operationState, msg, line, GetFileName(file));
        }

        [Event(CriticalExceptionEventId, Version = EventVersion, Level = EventLevel.Error, Keywords = Keywords.Debug)]
        protected override void WriteCriticalException(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file)
        {
            this.WriteEvent(CriticalEventId, trackingId, callerId, callerState, operation, operationState, msg, line, GetFileName(file));
        }

        [Event(UnexpectedEventId, Version = EventVersion, Level = EventLevel.Critical, Keywords = Keywords.Debug)]
        protected override void EventTrackingUnexpected(string trackingId, string callerId, string callerState, string operation, string operationState, string msg, int line, string file)
        {
            this.WriteEvent(UnexpectedEventId, trackingId, callerId, callerState, operation, operationState, msg, line, GetFileName(file));
        }
    }
}
