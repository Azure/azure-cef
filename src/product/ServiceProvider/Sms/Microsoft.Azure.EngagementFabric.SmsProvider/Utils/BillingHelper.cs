// <copyright file="BillingHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Microsoft.Azure.EngagementFabric.SmsProvider.Credential;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Utils
{
    public static class BillingHelper
    {
        public const int SingleMessageSize = 70;
        public const int MultipleMessageSize = 67;

        public static int CalculateBillingUnits(string message, ConnectorMetadata connectorMetadata)
        {
            // By default take as single report and charge once
            if (connectorMetadata == null || !connectorMetadata.SingleReportForLongMessage)
            {
                return 1;
            }

            // Calculate units by message length
            return GetTotalSegments(message);
        }

        public static int GetTotalSegments(string message)
        {
            if (message == null || message.Length <= SingleMessageSize)
            {
                return 1;
            }
            else
            {
                return (int)Math.Ceiling((double)message.Length / MultipleMessageSize);
            }
        }
    }
}
