// <copyright file="ResourceUsageType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Billing.Common.Contract
{
    [DataContract]
    public enum ResourceUsageType
    {
        [EnumMember]
        SmsTriggeredMessage,

        [EnumMember]
        SmsOtpMessage,

        [EnumMember]
        SmsCampaignMessage,

        [EnumMember]
        EmailMessage,

        [EnumMember]
        StandardPlan,

        [EnumMember]
        PremiumPlan
    }
}
