// -----------------------------------------------------------------------
// <copyright file="ServiceConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.Email.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Configuration
{
    public sealed class ServiceConfiguration
    {
        private readonly ICodePackageActivationContext context;

        public ServiceConfiguration(NodeContext nodeConext, ICodePackageActivationContext context)
        {
            this.context = context;
            this.DefaultConnectionString = this.context.GetConfig<string>("EmailProvider", "DefaultConnectionString");
            this.TelemetryStoreConnectionString = this.context.GetConfig<string>("EmailProvider", "TelemetryStoreConnectionString");
            this.MdmAccount = this.context.GetConfig<string>("EmailProvider", "MdmAccount");
            this.MdmMetricNamespace = this.context.GetConfig<string>("EmailProvider", "MdmMetricNamespace");
            this.Cluster = this.context.GetConfig<string>("EmailProvider", "Cluster");
            this.NodeName = nodeConext.NodeName;
            this.DispatchPartitionCount = this.context.GetConfig<int>("EmailProvider", "DispatchPartitionCount");
            this.ActorAccountMaxCount = this.context.GetConfig<int>("EmailProvider", "ActorAccountMaxCount");
            this.ActorReportMaxCount = this.context.GetConfig<int>("EmailProvider", "ActorReportMaxCount");
        }

        public string MdmAccount { get; set; }

        public string MdmMetricNamespace { get; set; }

        public string Cluster { get; set; }

        public string NodeName { get; set; }

        public string DefaultConnectionString { get; set; }

        public string TelemetryStoreConnectionString { get; set; }

        public int DispatchPartitionCount { get; set; }

        public int ActorAccountMaxCount { get; set; }

        public int ActorReportMaxCount { get; set; }
    }
}
