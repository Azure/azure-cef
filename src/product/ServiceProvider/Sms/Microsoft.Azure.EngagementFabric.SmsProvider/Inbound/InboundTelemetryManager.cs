// <copyright file="InboundTelemetryManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Configuration;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Inbound
{
    public class InboundTelemetryManager : IInboundTelemetryManager
    {
        private static readonly string InboundMessageTableName = "smsinbound";

        private CloudTableClient client;

        public InboundTelemetryManager(ServiceConfiguration configuration)
        {
            var account = CloudStorageAccount.Parse(configuration.TelemetryStoreConnectionString);
            this.client = account.CreateCloudTableClient();

            this.client.GetTableReference(InboundMessageTableName).CreateIfNotExists();
        }

        public async Task InsertInboundMessagesAsync(Signature signature, List<InboundMessage> messages, string extendedCode)
        {
            if (signature == null || messages == null || messages.Count <= 0)
            {
                return;
            }

            var table = this.client.GetTableReference(InboundMessageTableName);
            var entities = messages.Where(m => m.MoMessage != null).Select(m => new InboundMessageTableEntity(signature.EngagementAccount, signature.Value, m, extendedCode)).ToList();
            await InboundMessageTableEntity.InsertOrMergeBatchAsync(table, entities);
        }

        public async Task<List<InboundMessageTableEntity>> GetMessagesAsync(string engagementAccount, DateTime startTime, int count)
        {
            var table = this.client.GetTableReference(InboundMessageTableName);
            return await InboundMessageTableEntity.ListAsync(table, engagementAccount, startTime, count);
        }

        public async Task DeleteMessagesAsync(List<InboundMessageTableEntity> messages)
        {
            var table = this.client.GetTableReference(InboundMessageTableName);
            await InboundMessageTableEntity.DeleteAsync(table, messages);
        }

        public async Task DeleteMessagesAsync(string engagementAccount)
        {
            var table = this.client.GetTableReference(InboundMessageTableName);
            await InboundMessageTableEntity.DeleteAsync(table, engagementAccount);
        }

        public async Task CleanupMessageAsync(DateTime before)
        {
            var table = this.client.GetTableReference(InboundMessageTableName);
            await InboundMessageTableEntity.CleanupAsync(table, before);
        }
    }
}
