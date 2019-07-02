// <copyright file="PerMessageAggregationList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Microsoft.Azure.EngagementFabric.SmsProvider.Report;
using Microsoft.Azure.EngagementFabric.SmsProvider.Utils;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class PerMessageAggregationList
    {
        public PerMessageAggregationList(
            IEnumerable<PerMessageAggregation> values,
            DateTimeOffset startTime,
            DateTimeOffset endTime,
            TableContinuationToken continuationToken)
        {
            this.Values = values;
            this.StartTimeUnixSeconds = startTime.ToUnixTimeSeconds();
            this.EndTimeUnixSeconds = endTime.ToUnixTimeSeconds();
            this.ContinuationToken = continuationToken;
        }

        [JsonProperty("values")]
        public IEnumerable<PerMessageAggregation> Values { get; private set; }

        [JsonProperty("startTime")]
        public long StartTimeUnixSeconds { get; private set; }

        [JsonProperty("endTime")]
        public long EndTimeUnixSeconds { get; private set; }

        [JsonIgnore]
        public TableContinuationToken ContinuationToken { get; private set; }

        public class PerMessageAggregation
        {
            public PerMessageAggregation(
                MessageHistoryTableEntity historyEntity,
                IReadOnlyDictionary<string, int> countByState)
            {
                this.MessageId = historyEntity.MessageId;
                this.MessageCategory = historyEntity.MessageCategory;
                this.MessageBody = historyEntity.MessageBody;
                this.SendTimeUnixSeconds = new DateTimeOffset(historyEntity.SendTime).ToUnixTimeSeconds();
                this.TotalTargets = historyEntity.Targets * BillingHelper.GetTotalSegments(historyEntity.MessageBody);

                if (countByState.TryGetValue(MessageState.DELIVERED.ToString(), out int delivered))
                {
                    this.TotalSucceeded = delivered;
                }
                else
                {
                    this.TotalSucceeded = 0;
                }
            }

            [JsonProperty("messageId")]
            public string MessageId { get; private set; }

            [JsonProperty("messageCategory")]
            public string MessageCategory { get; private set; }

            [JsonProperty("messageBody")]
            public string MessageBody { get; private set; }

            [JsonProperty("sendTime")]
            public long SendTimeUnixSeconds { get; private set; }

            [JsonProperty("totalTargets")]
            public int TotalTargets { get; private set; }

            [JsonProperty("totalSucceeded")]
            public int TotalSucceeded { get; private set; }
        }
    }
}
