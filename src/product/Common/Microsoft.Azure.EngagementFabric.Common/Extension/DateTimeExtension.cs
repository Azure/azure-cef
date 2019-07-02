// <copyright file="DateTimeExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.Common.Extension
{
    public static class DateTimeExtension
    {
        public static DateTime ToLocal(this DateTime utcTime, string timeZoneInfo = null)
        {
            var info = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfo ?? Constants.TimeZoneInfo);
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, info);
        }

        public static DateTime ToUtc(this DateTime localTime, string timeZoneInfo = null)
        {
            var info = TimeZoneInfo.FindSystemTimeZoneById(timeZoneInfo ?? Constants.TimeZoneInfo);
            return TimeZoneInfo.ConvertTimeToUtc(localTime, info);
        }
    }
}
