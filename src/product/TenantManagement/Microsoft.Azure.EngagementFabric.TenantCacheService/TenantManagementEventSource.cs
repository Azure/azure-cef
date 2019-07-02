// <copyright file="TenantManagementEventSource.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;

namespace Microsoft.Azure.EngagementFabric.TenantCacheService
{
    [EventSource(Name = "Microsoft-Azure-EngagementFabric-TenantManagement")]
    [SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:ElementsMustAppearInTheCorrectOrder", Justification = "Allow EventId constants to be defined next to the associated event method")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsMustBeSeparatedByBlankLine", Justification = "Allow EventId constants to be defined next to the associated event method")]
    internal sealed class TenantManagementEventSource : EventSource
    {
        public static readonly TenantManagementEventSource Current = new TenantManagementEventSource();

        static TenantManagementEventSource()
        {
            // A workaround for the problem where ETW activities do not get tracked until Tasks infrastructure is initialized.
            // This problem will be fixed in .NET Framework 4.6.2.
            Task.Run(() => { });
        }

        // Instance constructor is private to enforce singleton semantics
        private TenantManagementEventSource()
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
            public const EventKeywords RestApi = (EventKeywords)0x2L;
        }
        #endregion

        #region Events
        private const int InfoEventId = 102;
        private const int WarningEventId = 103;
        private const int ErrorEventId = 104;

        private const int ActionBeginEventId = 301;
        private const int ActionEndEventId = 302;

        private const int TenantCacheSetEventId = 401;
        private const int TenantCacheDeleteEventId = 402;
        private const int TenantChangePublishEventId = 403;

        private const int QuotaPullEventId = 501;
        private const int QuotaPushEventId = 502;
        private const int QuotaNotFoundEventId = 503;
        private const int QuotaSyncSkippedEventId = 504;
        private const int QuotaSyncTimeoutEventId = 505;

        [Event(InfoEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] {2}")]
        public void TraceInfo(string trackingId, string nodeName, string content)
        {
            this.WriteEvent(InfoEventId, trackingId, nodeName, content);
        }

        [Event(WarningEventId, Level = EventLevel.Warning, Keywords = Keywords.Debug, Message = "[{0}] {2}")]
        public void TraceWarning(string trackingId, string nodeName, string content)
        {
            this.WriteEvent(WarningEventId, trackingId, nodeName, content);
        }

        [Event(ErrorEventId, Level = EventLevel.Error, Keywords = Keywords.Debug, Message = "[{0}] {2}")]
        public void TraceError(string trackingId, string nodeName, string content)
        {
            this.WriteEvent(ErrorEventId, trackingId, nodeName, content);
        }

        [Event(ActionBeginEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] ActionBegin {1}")]
        public void ActionBegin(string trackingId, string action, string accountName, string message)
        {
            this.WriteEvent(ActionBeginEventId, trackingId, action, accountName, message);
        }

        [Event(ActionEndEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] ActionEnd {1}")]
        public void ActionEnd(string trackingId, string action)
        {
            this.WriteEvent(ActionEndEventId, trackingId, action);
        }

        [Event(TenantCacheSetEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] {1} set {2}")]
        public void TenantCacheSet(string trackingId, string action, string accountName)
        {
            this.WriteEvent(TenantCacheSetEventId, trackingId, action, accountName);
        }

        [Event(TenantCacheDeleteEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] {1} delete {2}")]
        public void TenantCacheDelete(string trackingId, string action, string accountName)
        {
            this.WriteEvent(TenantCacheDeleteEventId, trackingId, action, accountName);
        }

        [Event(TenantChangePublishEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] {1} publish event {2} {3}")]
        public void TenantChangePublish(string trackingId, string action, string accountName, string eventType)
        {
            this.WriteEvent(TenantChangePublishEventId, trackingId, action, accountName, eventType);
        }

        [Event(QuotaPullEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] {1} pull {2} = {3}, reminding = {4}")]
        public void QuotaPull(string trackingId, string nodeName, string quotaId, int remindingInDB, int reminding)
        {
            this.WriteEvent(QuotaPullEventId, trackingId, nodeName, quotaId, remindingInDB, reminding);
        }

        [Event(QuotaPushEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] {1} push {2} = {3}")]
        public void QuotaPush(string trackingId, string nodeName, string quotaId, int reminding)
        {
            this.WriteEvent(QuotaPushEventId, trackingId, nodeName, quotaId, reminding);
        }

        [Event(QuotaNotFoundEventId, Level = EventLevel.Warning, Keywords = Keywords.Debug, Message = "[{0}] {1} cannot find {2}")]
        public void QuotaNotFound(string trackingId, string nodeName, string quotaId)
        {
            this.WriteEvent(QuotaNotFoundEventId, trackingId, nodeName, quotaId);
        }

        [Event(QuotaSyncSkippedEventId, Level = EventLevel.Informational, Keywords = Keywords.Debug, Message = "[{0}] {1} synchronizing of {2} skipped")]
        public void QuotaSyncSkipped(string trackingId, string nodeName, string quotaId)
        {
            this.WriteEvent(QuotaSyncSkippedEventId, trackingId, nodeName, quotaId);
        }

        [Event(QuotaSyncTimeoutEventId, Level = EventLevel.Warning, Keywords = Keywords.Debug, Message = "[{0}] {1} synchronizing of {2} timeout")]
        public void QuotaSyncTimeout(string trackingId, string nodeName, string quotaId)
        {
            this.WriteEvent(QuotaSyncTimeoutEventId, trackingId, nodeName, quotaId);
        }

        [NonEvent]
        public void TraceException(string trackingId, string nodeName, string content, Exception ex)
        {
            this.TraceError(trackingId, nodeName, EventSourceHelper.FormatMessageAndException(content, ex));
        }
        #endregion
    }
}
