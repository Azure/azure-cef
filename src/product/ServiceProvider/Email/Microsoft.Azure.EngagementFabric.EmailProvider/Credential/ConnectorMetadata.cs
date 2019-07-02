// <copyright file="ConnectorMetadata.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Credential
{
    public class ConnectorMetadata
    {
        public string ConnectorName { get; set; }

        public string ConnectorUri { get; set; }

        public long BatchSize { get; set; }
    }
}
