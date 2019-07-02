// <copyright file="Meter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.BillingService.Manager
{
    public class Meter
    {
        public static readonly Meter MeterOfSmsTriggeredMessage = new Meter
        {
            MeterId = "38288822-aaaf-41df-b9df-f47ab261887d",
            MeterUnit = 1
        };

        public static readonly Meter MeterOfSmsOtpMessage = new Meter
        {
            MeterId = "57f366d8-181c-46b3-8150-d6646d78e48d",
            MeterUnit = 1
        };

        public static readonly Meter MeterOfSmsCampaignMessage = new Meter
        {
            MeterId = "f803e412-a6d0-4fb8-910d-a496bbe5e0c4",
            MeterUnit = 1
        };

        public static readonly Meter MeterOfEmailMessage = new Meter
        {
            MeterId = "6222e8ca-b81b-4640-9d71-390e2ab90d9f",
            MeterUnit = 1
        };

        public static readonly Meter MeterOfStandardPlan = new Meter
        {
            MeterId = "25e2984d-9daa-4dda-8439-55ef359d361a",
            MeterUnit = 1
        };

        public static readonly Meter MeterOfPremiumPlan = new Meter
        {
            MeterId = "d72f4fe7-25bd-49c1-b8ad-5dbdba12fe12",
            MeterUnit = 1
        };

        public static readonly Dictionary<ResourceUsageType, Meter> MeterMappings = new Dictionary<ResourceUsageType, Meter>
        {
            { ResourceUsageType.SmsTriggeredMessage, Meter.MeterOfSmsTriggeredMessage },
            { ResourceUsageType.SmsOtpMessage, Meter.MeterOfSmsOtpMessage },
            { ResourceUsageType.SmsCampaignMessage, Meter.MeterOfSmsCampaignMessage },
            { ResourceUsageType.EmailMessage, Meter.MeterOfEmailMessage },
            { ResourceUsageType.StandardPlan, Meter.MeterOfStandardPlan },
            { ResourceUsageType.PremiumPlan, Meter.MeterOfPremiumPlan }
        };

        public string MeterId { get; set; }

        public int MeterUnit { get; set; }
    }
}
