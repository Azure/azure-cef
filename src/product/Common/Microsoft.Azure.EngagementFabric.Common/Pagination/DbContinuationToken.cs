// <copyright file="DbContinuationToken.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.EngagementFabric.Common.Pagination
{
    public class DbContinuationToken : ContinuationToken
    {
        public DbContinuationToken(int databaseId, int skip)
        {
            var str = JsonConvert.SerializeObject(new
            {
                DatabaseId = databaseId,
                Skip = skip
            });

            this.Token = ToBase64UriEscapeString(str);
            this.IsValid = true;
        }

        public DbContinuationToken(string continuationTokenString)
        {
            this.IsValid = false;

            // empty or whiteSpace is treat as valid continuationToken
            if (string.IsNullOrWhiteSpace(continuationTokenString))
            {
                this.IsValid = true;
                return;
            }

            var originalToken = FromBase64UriEscapeString(continuationTokenString);
            if (originalToken == null)
            {
                return;
            }

            try
            {
                var obj = JObject.Parse(originalToken);
                this.DatabaseId = obj?["DatabaseId"]?.Value<int>() ?? 0;
                this.Skip = obj?["Skip"]?.Value<int>() ?? 0;
                this.Token = continuationTokenString;
                this.IsValid = true;
            }
            catch
            {
            }
        }

        public int DatabaseId { get; set; }

        public int Skip { get; set; }
    }
}
