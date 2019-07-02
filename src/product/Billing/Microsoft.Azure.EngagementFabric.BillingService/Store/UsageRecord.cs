// <copyright file="UsageRecord.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;
using Microsoft.Azure.EngagementFabric.BillingService.Manager;
using Microsoft.Azure.EngagementFabric.TenantCache.Contract;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.BillingService.Store
{
    public class UsageRecord : TableEntity
    {
        public UsageRecord()
        {
        }

        public UsageRecord(ResourceUsageRecord record, Tenant tenant, Guid batchId)
        {
            var meter = Meter.MeterMappings[record.UsageType];

            this.PartitionKey = batchId.ToString();
            this.RowKey = $"{record.EngagementAccount}:{record.UsageType.ToString()}";

            this.SubscriptionId = Guid.Parse(tenant.SubscriptionId);
            this.EventId = Guid.NewGuid();
            this.EventDateTime = DateTime.UtcNow;
            this.Quantity = record.Quantity / meter.MeterUnit;
            this.MeterId = meter.MeterId;
            this.ResourceUri = tenant.ResourceId;
            this.Location = tenant.Location;
            this.TagsDictionary = tenant.Tags;
            this.AdditionalInfoDictionary = new Dictionary<string, string>
            {
                { nameof(tenant.SKU), tenant.SKU },
                { nameof(tenant.State), tenant.State.ToString() }
            };

            this.SerializeCollections();
        }

        public Guid SubscriptionId { get; set; }

        public Guid EventId { get; set; }

        public DateTime EventDateTime { get; set; }

        public double Quantity { get; set; }

        public string MeterId { get; set; }

        public string ResourceUri { get; set; }

        public string Location { get; set; }

        public string Tags { get; set; }

        [IgnoreProperty]
        public IDictionary<string, string> TagsDictionary { get; set; }

        public string PartNumber { get; set; }

        public string OrderNumber { get; set; }

        public string AdditionalInfo { get; set; }

        [IgnoreProperty]
        public IDictionary<string, string> AdditionalInfoDictionary { get; set; }

        public void SerializeCollections()
        {
            Tags = SerializeDictionary(TagsDictionary);
            AdditionalInfo = SerializeDictionary(AdditionalInfoDictionary);
        }

        private string SerializeDictionary(IDictionary<string, string> dictionary)
        {
            if (dictionary == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(dictionary, Formatting.None);
        }
    }
}
