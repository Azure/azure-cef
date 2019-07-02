// <copyright file="AzureStorageContinuationToken.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Common.Pagination
{
    public class AzureStorageContinuationToken : ContinuationToken
    {
        public AzureStorageContinuationToken(TableContinuationToken tableContinuationToken)
        {
            if (tableContinuationToken != null)
            {
                var str = JsonConvert.SerializeObject(tableContinuationToken);
                this.Token = ToBase64UriEscapeString(str);
            }

            this.IsValid = true;
        }

        public AzureStorageContinuationToken(string continuationTokenString)
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

            this.TableContinuationToken = JsonConvert.DeserializeObject<TableContinuationToken>(originalToken);
            this.Token = continuationTokenString;
            this.IsValid = true;
        }

        public TableContinuationToken TableContinuationToken { get; set; }
    }
}
