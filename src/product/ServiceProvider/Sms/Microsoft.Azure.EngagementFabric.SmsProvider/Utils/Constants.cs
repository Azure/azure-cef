// <copyright file="Constants.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Billing.Common.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Utils
{
    public static class Constants
    {
        public const int SignatureMinLength = 2;
        public const int SignatureMaxLength = 12;
        public const int TemplateBodyMinLength = 1;
        public const int TemplateBodyMaxLength = 500;
        public const int SmsBodyMinLength = 1;
        public const int SmsBodyMaxLength = 500;
        public const int TargetMaxSize = 500;
        public const int ExtendedCodeCompanyLength = 2;
        public const int ExtendedCodeSignatureLength = 3;
        public const int ExtendedCodeCustomLength = 2;
        public const int PromotionMinHour = 8;
        public const int PromotionMaxHour = 21;
        public const int PagingMaxTakeCount = 500;

        public const string TemplatePlaceHolderFormat = "$({0})";
        public const string TemplatePlaceHolderRegex = "\\$\\(.*?\\)";
        public const string SmsBodyFormat = "【{0}】{1}";
        public const string SmsQuotaName = "SMS";

        public static readonly string DispatcherServiceUri = "fabric:/SmsApp/MessageDispatcher";
        public static readonly string ReportingServiceUri = "fabric:/SmsApp/SmsProvider";

        public static readonly Dictionary<MessageCategory, List<ChannelType>> MessageAllowedChannelMappings = new Dictionary<MessageCategory, List<ChannelType>>
        {
            { MessageCategory.Notification, new List<ChannelType> { ChannelType.Industry, ChannelType.Both } },
            { MessageCategory.Otp, new List<ChannelType> { ChannelType.Industry, ChannelType.Both } },
            { MessageCategory.Promotion, new List<ChannelType> { ChannelType.Marketing, ChannelType.Both } }
        };

        public static readonly Dictionary<MessageCategory, ChannelType> MessageSendChannelMappings = new Dictionary<MessageCategory, ChannelType>
        {
            { MessageCategory.Notification, ChannelType.Industry },
            { MessageCategory.Otp, ChannelType.Industry },
            { MessageCategory.Promotion, ChannelType.Marketing }
        };

        public static readonly List<int> ExtendedCodeSegmentLengths = new List<int>
        {
            ExtendedCodeCompanyLength,
            ExtendedCodeSignatureLength,
            ExtendedCodeCustomLength
        };

        public static readonly Dictionary<MessageCategory, ResourceUsageType> MessageCategoryToUsageTypeMappings = new Dictionary<MessageCategory, ResourceUsageType>
        {
            { MessageCategory.Notification, ResourceUsageType.SmsTriggeredMessage },

            // OTP message will use triggered message meter by current design
            { MessageCategory.Otp, ResourceUsageType.SmsTriggeredMessage },
            { MessageCategory.Promotion, ResourceUsageType.SmsCampaignMessage }
        };
    }
}
