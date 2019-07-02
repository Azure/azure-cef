// <copyright file="Constants.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Utils
{
    public static class Constants
    {
        public const int DomainMinLength = 10;
        public const int DomainMaxLength = 50;
        public const int TemplateBodyMinLength = 1;
        public const int TemplateBodyMaxLength = 16777215;
        public const int EmailBodyMinLength = 1;
        public const int EmailBodyMaxLength = 16777215;
        public const int TargetMaxSize = 1000;
        public const int PagingMaxTakeCount = 500;
        public const int MailingMaxTargets = 500;

        public const int ReportPullingIntervalByMinutes = 60;
        public const int ReportInProgressIntervalByHours = 24;

        public const string TemplatePlaceHolderFormat = "$({0})";
        public const string TemplatePlaceHolderRegex = "\\$\\(.*?\\)";

        public static readonly string DispatcherServiceUri = "fabric:/EmailApp/MessageDispatcher";
        public static readonly string ReportingServiceUri = "fabric:/EmailApp/EmailProvider";
    }
}
