// <copyright file="OtpHistoryTableEntity.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Telemetry
{
    public class OtpHistoryTableEntity : TableEntity
    {
        public OtpHistoryTableEntity()
            : base()
        {
        }

         public OtpHistoryTableEntity(string account, string phoneNumber, string action, DateTime timeStart)
        {
            this.PartitionKey = timeStart.ToUniversalTime().Ticks.ToString();
            this.RowKey = phoneNumber;
            Account = account;
            PhoneNumber = phoneNumber;
            ActionTime = timeStart;
            Action = action;
        }

        public string Account { get; set; }

        public string PhoneNumber { get; set; }

        public string Action { get; set; }

        public DateTime ActionTime { get; set; }

        internal static async Task InsertOtpLoginHistoryTableEntity(CloudTable table, OtpHistoryTableEntity summary)
        {
                var operation = TableOperation.Insert(summary);
                var tempinvert = table.ExecuteAsync(operation);
                await Task.CompletedTask;
        }
    }
}
