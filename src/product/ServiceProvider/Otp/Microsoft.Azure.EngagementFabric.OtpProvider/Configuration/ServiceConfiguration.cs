// -----------------------------------------------------------------------
// <copyright file="ServiceConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Text.RegularExpressions;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.OtpProvider.Utils;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Configuration
{
    public sealed class ServiceConfiguration
    {
        private readonly ICodePackageActivationContext context;

        public ServiceConfiguration(NodeContext nodeConext, ICodePackageActivationContext context)
        {
            this.context = context;
            this.DefaultConnectionPoolSize = this.context.GetConfig<string>("OtpProvider", "DefaultConnectionPoolSize");
            this.DefaultConnectionString = this.context.GetConfig<string>("OtpProvider", "DefaultConnectionString");
            this.TelemetryStoreConnectionString = this.context.GetConfig<string>("OtpProvider", "TelemetryStoreConnectionString");
            this.MdmAccount = this.context.GetConfig<string>("OtpProvider", "MdmAccount");
            this.MdmMetricNamespace = this.context.GetConfig<string>("OtpProvider", "MdmMetricNamespace");
            this.Cluster = this.context.GetConfig<string>("OtpProvider", "Cluster");
            this.NodeName = nodeConext.NodeName;
        }

        public string MdmAccount { get; set; }

        public string MdmMetricNamespace { get; set; }

        public string Cluster { get; set; }

        public string NodeName { get; set; }

        public string DefaultConnectionPoolSize { get; set; }

        public string DefaultConnectionString { get; set; }

        public string TelemetryStoreConnectionString { get; set; }
    }
}
