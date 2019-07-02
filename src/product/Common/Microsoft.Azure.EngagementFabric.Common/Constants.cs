// <copyright file="Constants.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.Common
{
    public class Constants
    {
        public const string PaymentProviderName = "payment";
        public const string SmsProviderName = "sms";
        public const string SocialProviderName = "social";
        public const string OtpProviderName = "otp";
        public const string EmailProviderName = "email";

        public const string AccountHeader = "Account";
        public const string OperationTrackingIdHeader = "x-ms-request-id";
        public const string QuotaRemainingHeader = "x-ms-quota-remaining";

        public const string TimeZoneInfo = "China Standard Time";

        // Quota names
        public const string SocialLoginMAU = "SocialLoginMAU";
        public const string SocialLoginTotal = "SocialLoginTotal";

        public const string SmsTotal = "Sms";
        public const string SmsSignatureDaily = "SmsSignatureDaily";
        public const string SmsSignatureMAUNamingTemplate = SmsSignatureDaily + "-{0}";
        public const string SmsSignatureMAUNamingRegex = SmsSignatureDaily + "-(.*)";
    }
}
