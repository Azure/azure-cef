// <copyright file="SecurityEventSource.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    [EventSource(Name = "Microsoft-Azure-EngagementFabric-Security")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Allow EventId constants to be defined next to the associated event method")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsMustBeSeparatedByBlankLine", Justification = "Allow EventId constants to be defined next to the associated event method")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1001:CommasMustBeSpacedCorrectly", Justification = "Allow EventAttribute with Message conditionally compiled")]
    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "Allow EventAttribute with Message conditionally compiled")]
    public class SecurityEventSource : EventSource
    {
        public static readonly SecurityEventSource Current = new SecurityEventSource();
        public static readonly string EmptyTrackingId = string.Empty;

        static SecurityEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        // Instance constructor is private to enforce singleton semantics
        private SecurityEventSource()
            : base()
        {
        }

        #region Keywords

        // Event keywords can be used to categorize events.
        // Each keyword is a bit flag. A single event can be associated with multiple keywords (via EventAttribute.Keywords property).
        // Keywords must be defined as a public class named 'Keywords' inside EventSource that uses them.
        public static class Keywords
        {
            public const EventKeywords SSL = (EventKeywords)0x1L;
            public const EventKeywords Audit = (EventKeywords)0x2L;
        }
        #endregion

        #region Events

        private const int ConnectionLoggingId = 101;
        private const int AuditFailureEventId = 102;
        private const int AuditExceptionEventId = 103;

        [Event(ConnectionLoggingId, Level = EventLevel.Informational, Keywords = Keywords.SSL)]
        public void TraceConnectionLogging(string provider, string message)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(ConnectionLoggingId, provider, message);
            }
        }

        [Event(AuditFailureEventId, Level = EventLevel.Error, Keywords = Keywords.Audit)]
        public void TraceAuditFailure(string requestId, string path)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(AuditFailureEventId, requestId, path);
            }
        }

        [Event(AuditExceptionEventId, Level = EventLevel.Error, Keywords = Keywords.Audit)]
        public void TraceAuditException(string requestId, string path, string message)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(AuditExceptionEventId, requestId, path, message);
            }
        }

        #endregion

        #region Private methods
#if UNSAFE
        private int SizeInBytes(string s)
        {
            if (s == null)
            {
                return 0;
            }
            else
            {
                return (s.Length + 1) * sizeof(char);
            }
        }
#endif
        #endregion
    }
}
