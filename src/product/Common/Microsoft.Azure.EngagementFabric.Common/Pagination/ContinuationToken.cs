// <copyright file="ContinuationToken.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Text;

namespace Microsoft.Azure.EngagementFabric.Common.Pagination
{
    public abstract class ContinuationToken
    {
        public const string ContinuationTokenKey = "ContinuationToken";
        public const int DefaultCount = 100;

        public string Token { get; set; }

        public bool IsValid { get; set; }

        public static string ToBase64UriEscapeString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return Uri.EscapeDataString(Convert.ToBase64String(Encoding.UTF8.GetBytes(value)));
        }

        public static string FromBase64UriEscapeString(string base64Value)
        {
            if (string.IsNullOrWhiteSpace(base64Value))
            {
                return null;
            }

            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(Uri.UnescapeDataString(base64Value)));
            }
            catch (ArgumentNullException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
