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
using Microsoft.Azure.EngagementFabric.SocialProvider.Utils;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Configuration
{
    public sealed class ServiceConfiguration
    {
        private readonly ICodePackageActivationContext context;

        public ServiceConfiguration(NodeContext nodeConext, ICodePackageActivationContext context)
        {
            this.context = context;
            this.DefaultConnectionString = this.context.GetConfig<string>("SocialProvider", "DefaultConnectionString");
            this.MdmAccount = this.context.GetConfig<string>("SocialProvider", "MdmAccount");
            this.MdmMetricNamespace = this.context.GetConfig<string>("SocialProvider", "MdmMetricNamespace");
            this.Cluster = this.context.GetConfig<string>("SocialProvider", "Cluster");
            this.NodeName = nodeConext.NodeName;
        }

        public string MdmAccount { get; set; }

        public string MdmMetricNamespace { get; set; }

        public string Cluster { get; set; }

        public string NodeName { get; set; }

        public string DefaultConnectionString { get; set; }

        public string TelemetryStoreConnectionString { get; set; }
    }
}
