// <copyright file="TradeModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public class TradeModel
    {
        public int? Amount { get; set; }

        public DateTimeOffset TimeStart { get; set; }

        public DateTimeOffset? TimeExpire { get; set; }

        public DateTimeOffset? TimeEnd { get; set; }

        public TradeState TradeState { get; set; }

        public Dictionary<string, string> PrepayParameters { get; set; }
    }
}