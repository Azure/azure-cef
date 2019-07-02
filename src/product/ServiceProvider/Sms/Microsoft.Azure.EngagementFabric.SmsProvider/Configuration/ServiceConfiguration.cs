// -----------------------------------------------------------------------
// <copyright file="ServiceConfiguration.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Configuration
{
    public sealed class ServiceConfiguration
    {
        private readonly ICodePackageActivationContext context;

        public ServiceConfiguration(NodeContext nodeConext, ICodePackageActivationContext context)
        {
            this.context = context;
            this.DefaultConnectionString = this.context.GetConfig<string>("SmsProvider", "DefaultConnectionString");
            this.TelemetryStoreConnectionString = this.context.GetConfig<string>("SmsProvider", "TelemetryStoreConnectionString");
            this.MdmAccount = this.context.GetConfig<string>("SmsProvider", "MdmAccount");
            this.MdmMetricNamespace = this.context.GetConfig<string>("SmsProvider", "MdmMetricNamespace");
            this.Cluster = this.context.GetConfig<string>("SmsProvider", "Cluster");
            this.NodeName = nodeConext.NodeName;

            this.DispatchPartitions = new List<DispatchPartition>();

            // Parse dispatch partitions, the string should be in format e.g. "[Category1:0:4][Category2:5:6][Category3:7:7]"
            var partitionString = this.context.GetConfig<string>("SmsProvider", "DispatchPartitions");
            var regex = new Regex(@"\[([a-zA-Z]+):([0-9]+):([0-9]+)\]");
            var results = regex.Matches(partitionString);
            if (results.Count > 0)
            {
                foreach (Match result in results)
                {
                    if (Enum.TryParse(result.Groups[1].Value, out MessageCategory category) &&
                        int.TryParse(result.Groups[2].Value, out int min) &&
                        int.TryParse(result.Groups[3].Value, out int max))
                    {
                        this.DispatchPartitions.Add(new DispatchPartition(category, min, max));
                    }
                    else
                    {
                        SmsProviderEventSource.Current.Warning(SmsProviderEventSource.EmptyTrackingId, this, "Construction", OperationStates.FailedNotFaulting, $"Invalid category '{result.Groups[1].Value}' in configuration.");
                    }
                }
            }

            // Parse Ops info
            this.SmsOpsInfo = new OpsInfo();
            this.SmsOpsInfo.SenderAddress = this.context.GetConfig<string>("SmsProvider", "SenderAddress");
            this.SmsOpsInfo.SenderPassword = this.context.GetConfig<string>("SmsProvider", "SenderPassword");

            var receivers = this.context.GetConfig<string>("SmsProvider", "ReceiverAddresses");
            this.SmsOpsInfo.ReceiverAddresses = !string.IsNullOrEmpty(receivers) ?
                receivers.Split(';')?.ToList() :
                new List<string>();

            if (!string.IsNullOrEmpty(this.SmsOpsInfo.SenderAddress))
            {
                this.SmsOpsInfo.ReceiverAddresses.Add(this.SmsOpsInfo.SenderAddress);
            }

            var thumbprint = this.context.GetConfig<string>("SmsProvider", "MdmCertificateThumbprint");
            this.MdmCertificate = FindCertificate(thumbprint);

            var interval = this.context.GetConfig<string>("SmsProvider", "MdmArchiveInterval");
            try
            {
                this.MdmArchiveInterval = XmlConvert.ToTimeSpan(interval);
            }
            catch
            {
                this.MdmArchiveInterval = TimeSpan.FromHours(1);
            }
        }

        public string MdmAccount { get; set; }

        public string MdmMetricNamespace { get; set; }

        public string Cluster { get; set; }

        public string NodeName { get; set; }

        public string DefaultConnectionString { get; set; }

        public string TelemetryStoreConnectionString { get; set; }

        public List<DispatchPartition> DispatchPartitions { get; set; }

        public OpsInfo SmsOpsInfo { get; set; }

        public X509Certificate2 MdmCertificate { get; private set; }

        public TimeSpan MdmArchiveInterval { get; private set; }

        private static X509Certificate2 FindCertificate(string thumbprint)
        {
            var store = new X509Store(StoreLocation.LocalMachine);

            try
            {
                store.Open(OpenFlags.ReadOnly);

                var collection = store.Certificates.Find(
                    X509FindType.FindByThumbprint,
                    thumbprint,
                    false);

                return collection.OfType<X509Certificate2>().FirstOrDefault();
            }
            catch
            {
                return null;
            }
            finally
            {
                store.Close();
            }
        }

        public class DispatchPartition
        {
            public DispatchPartition(MessageCategory category, int min, int max)
            {
                this.Category = category;
                this.MinPartition = min;
                this.MaxPartition = max;
            }

            public MessageCategory Category { get; set; }

            public int MinPartition { get; set; }

            public int MaxPartition { get; set; }
        }

        public class OpsInfo
        {
            public string SenderAddress { get; set; }

            public string SenderPassword { get; set; }

            public List<string> ReceiverAddresses { get; set; }

            public bool IsValid()
            {
                return !string.IsNullOrEmpty(this.SenderAddress) &&
                    !string.IsNullOrEmpty(this.SenderPassword) &&
                    this.ReceiverAddresses != null && this.ReceiverAddresses.Count > 0;
            }
        }
    }
}
