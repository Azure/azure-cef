// <copyright file="OtpChannelHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Helper
{
    public static class OtpChannelHelper
    {
        public const string Sms = "sms";

        public static string Format(string channel)
        {
            if (Sms.Equals(channel, StringComparison.OrdinalIgnoreCase))
            {
                return Sms;
            }

            throw new ArgumentException($"Channel '{channel}' is not supported.");
        }
    }
}