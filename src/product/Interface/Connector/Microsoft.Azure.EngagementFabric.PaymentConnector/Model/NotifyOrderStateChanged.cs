// <copyright file="NotifyOrderStateChanged.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public class NotifyOrderStateChanged
    {
        public Guid OrderId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TradeState TradeState { get; set; }

        public DateTimeOffset? TimeEnd { get; set; }
    }
}