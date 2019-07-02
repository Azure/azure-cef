// -----------------------------------------------------------------------
// <copyright file="ServiceConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Fabric;
using Microsoft.Azure.EngagementFabric.Common.Extension;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Manager
{
    public class ServiceConfiguration
    {
        private readonly ICodePackageActivationContext context;

        public ServiceConfiguration(NodeContext nodeConext, ICodePackageActivationContext context)
        {
            this.context = context;

            this.OnlyHttps = this.context.GetConfig<bool>("RequestListener", "OnlyHttps");
            this.MdmAccount = this.context.GetConfig<string>("RequestListener", "MdmAccount");
            this.MdmMetricNamespace = this.context.GetConfig<string>("RequestListener", "MdmMetricNamespace");
            this.Cluster = this.context.GetConfig<string>("RequestListener", "Cluster");
            this.NodeName = nodeConext.NodeName;
            this.AcisCertificateThumbprint = this.context.GetConfig<string>("RequestListener", "AcisCertificateThumbprint");
        }

        public bool OnlyHttps { get; set; }

        public string MdmAccount { get; set; }

        public string MdmMetricNamespace { get; set; }

        public string Cluster { get; set; }

        public string NodeName { get; set; }

        public string AcisCertificateThumbprint { get; set; }
    }
}