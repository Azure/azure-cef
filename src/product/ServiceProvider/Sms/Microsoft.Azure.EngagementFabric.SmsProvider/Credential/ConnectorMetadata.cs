// <copyright file="ConnectorMetadata.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Credential
{
    public class ConnectorMetadata
    {
        public enum ConnectorInboundType
        {
            NotSupport,
            Pull,
            Push
        }

        public string ConnectorName { get; set; }

        public string ConnectorUri { get; set; }

        public long BatchSize { get; set; }

        public ConnectorInboundType ReportType { get; set; }

        public ConnectorInboundType InboundMessageType { get; set; }

        public bool SingleReportForLongMessage { get; set; }
    }
}
