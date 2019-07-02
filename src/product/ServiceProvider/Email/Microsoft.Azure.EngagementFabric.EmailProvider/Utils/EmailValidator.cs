// <copyright file="EmailValidator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Microsoft.Azure.EngagementFabric.Common;
using EmailConstant = Microsoft.Azure.EngagementFabric.EmailProvider.Utils.Constants;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Utils
{
    public class EmailValidator
    {
        public static bool IsEmailValid(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public List<string> ValidateEmails(List<string> emails)
        {
            Validator.IsTrue<ArgumentException>(
                emails != null &&
                emails.Count > 0,
                nameof(emails),
                "Empty email adress");

            Validator.IsTrue<ArgumentException>(
                emails != null &&
                emails.Count <= EmailConstant.TargetMaxSize,
                nameof(emails),
                "Too many emails");

            var filtered = emails.Distinct().ToList();
            var invalid = filtered.Where(n => !IsEmailValid(n)).ToList();
            Validator.IsTrue<ArgumentException>(
                invalid.Count <= 0,
                nameof(invalid),
                "Emails formatted incorrectly: {0}",
                string.Join(",", invalid));

            return filtered;
        }
    }
}
