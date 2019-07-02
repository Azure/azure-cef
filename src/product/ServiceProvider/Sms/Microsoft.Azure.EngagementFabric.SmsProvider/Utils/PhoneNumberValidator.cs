// <copyright file="PhoneNumberValidator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Text.RegularExpressions;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Utils
{
    public class PhoneNumberValidator
    {
        public static bool IsNumberValid(string phoneNumber)
        {
            var regex = new Regex("^((\\+86)|(86)|(0086))?[1][0-9]{10}$");
            return regex.IsMatch(phoneNumber);
        }
    }
}
