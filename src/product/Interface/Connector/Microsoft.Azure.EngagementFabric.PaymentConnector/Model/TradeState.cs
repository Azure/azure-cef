// <copyright file="TradeState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.PaymentConnector
{
    public enum TradeState
    {
        Unknown = 0,
        Pending,
        NotPay,
        UserPayng,
        Success,
        Closed,
        Revoked,
        Refund,
        PayError,
        Finished
    }
}