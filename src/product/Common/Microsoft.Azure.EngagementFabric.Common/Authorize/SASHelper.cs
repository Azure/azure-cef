// <copyright file="SASHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Microsoft.Azure.EngagementFabric.Common.Authorize
{
    public static class SASHelper
    {
        public const string Schema = "SharedAccessSignature";

        private const string SignKey = "sig";
        private const string KeyNameKey = "skn";
        private const string ExpiryKey = "se";

        private static readonly string[] RequiredKeys = new string[] { SignKey, KeyNameKey, ExpiryKey };
        private static readonly Random Rand = new Random();

        public static string GenerateKey(int keyLength)
        {
            var bytes = new byte[keyLength];
            Rand.NextBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public static string CreateToken(string key, string keyName, TimeSpan timeout)
        {
            var values = new Dictionary<string, string>
            {
                { KeyNameKey, keyName },
                { ExpiryKey, (DateTimeOffset.UtcNow + timeout).ToUnixTimeSeconds().ToString() }
            };

            var signContent = GetSignContent(values);
            return $"{Schema} {SignKey}={HttpUtility.UrlEncode(Sign(signContent, key))}&{signContent}";
        }

        public static void ValidateToken(string token, IEnumerable<KeyValuePair<string, string>> keyPairs)
        {
            var collection = HttpUtility.ParseQueryString(token);
            var values = collection
                .Cast<string>()
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .ToDictionary(key => key, key => collection[key]);

            var firstMissingField = RequiredKeys.FirstOrDefault(key => !values.ContainsKey(key));
            if (firstMissingField != null)
            {
                throw new SASInvalidException($"Missing required SAS field '{firstMissingField}'");
            }

            DateTimeOffset expiry;
            try
            {
                expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(values[ExpiryKey]));
            }
            catch
            {
                throw new SASInvalidException("Invalid SAS signature");
            }

            if (DateTimeOffset.UtcNow > expiry)
            {
                throw new SASInvalidException("SAS is expired");
            }

            // ToDo: allow customized policies and query database to map policies to key names
            var keyName = values[KeyNameKey];

            var keys = keyPairs
                .Where(pair => string.Equals(pair.Key, keyName, StringComparison.InvariantCultureIgnoreCase))
                .Select(pair => pair.Value);
            if (!keys.Any())
            {
                throw new SASInvalidException("Invalid SAS key");
            }

            var sign = values[SignKey];
            var signContent = GetSignContent(values);
            if (!keys.Any(key => sign == Sign(signContent, key)))
            {
                throw new SASInvalidException("Invalid SAS signature");
            }
        }

        public static string GetKeyNameFromToken(string token)
        {
            var collection = HttpUtility.ParseQueryString(token);
            var values = collection
                .Cast<string>()
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .ToDictionary(key => key, key => collection[key]);

            return values[KeyNameKey];
        }

        private static string GetSignContent(IReadOnlyDictionary<string, string> values)
        {
            return string.Join("&", values
                .Where(pair => pair.Key != SignKey)
                .OrderBy(pair => pair.Key)
                .Select(pair => $"{pair.Key}={HttpUtility.UrlEncode(pair.Value)}"));
        }

        private static string Sign(string signContent, string key)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
            {
                var sign = hmac.ComputeHash(Encoding.UTF8.GetBytes(signContent));
                return Convert.ToBase64String(sign);
            }
        }
    }
}