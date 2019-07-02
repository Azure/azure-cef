// <copyright file="EventOrderStateChanged.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public class EventOrderStateChanged
    {
        /// <summary>
        /// Gets or sets the backend channel trade number extracted from the notify
        /// </summary>
        public string ChannelTradeNumber { get; set; }

        /// <summary>
        /// Gets or sets the trade state extracted from the notify
        /// </summary>
        public TradeState TradeState { get; set; }

        /// <summary>
        /// Gets or sets the trade end time extracted from the notify
        /// </summary>
        public DateTimeOffset? TimeEnd { get; set; }

        /// <summary>
        /// Gets or sets the media type of the response for backend channel
        /// Note: this field will be ignored if CEF failed to POST common notify to customer
        /// </summary>
        public string ResponseMediaType { get; set; }

        /// <summary>
        /// Gets or sets the connector desired response content for backend channel
        /// Note: this field will be ignored if CEF failed to POST common notify to customer
        /// </summary>
        public string ResponseContent { get; set; }
    }
}