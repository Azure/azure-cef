// <copyright file="IPaymentConnector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public interface IPaymentConnector
    {
        /// <summary>
        /// Gets the channel application ID
        /// </summary>
        string AppId { get; }

        /// <summary>
        /// Create order
        /// </summary>
        /// <param name="subject">Order subject</param>
        /// <param name="tradeNumber">Trade number (unique ID)</param>
        /// <param name="totalFee">Amount in 0.01 RMB</param>
        /// <param name="notifyUrl">Notify URL</param>
        /// <param name="timeStart">Start time in Unix seconds</param>
        /// <param name="timeExpire">Expire time in Unix seconds</param>
        /// <returns>
        /// 1. Order ID as unique ID in CEF
        /// 2. PrepayID from backend channel to launch mobile APP
        /// </returns>
        /// ToDo: add parameter `currency` for globalization
        /// ToDo: add parameters `body` and `description` to provider more details of the goods
        Task<TradeModel> CreateOrderAsync(
            string subject,
            string tradeNumber,
            int totalFee,
            string notifyUrl,
            long timeStart,
            long timeExpire);

        /// <summary>
        /// Retrieve order information from backend channel
        /// </summary>
        /// <param name="tradeNumber">Trade number used to create the order</param>
        /// <returns>
        /// 1. Common trade state
        /// 2. Trade end time in case trade completed
        /// </returns>
        Task<TradeModel> QueryOrderAsync(string tradeNumber);

        /// <summary>
        /// Close order which was not paid
        /// </summary>
        /// <param name="tradeNumber">Trade number used to create the order</param>
        /// <param name="notifyUrl">Notify URL</param>
        /// <returns>Async task</returns>
        Task CloseOrderAsync(string tradeNumber, string notifyUrl);

        /// <summary>
        /// Parse the backend channel specified notify message to common notify
        /// </summary>
        /// <param name="content">Backend channel notify content</param>
        /// <returns>Common notify</returns>
        EventOrderStateChanged ParseOrderStateChangedEvent(string content);
    }
}
