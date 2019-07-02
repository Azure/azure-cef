// <copyright file="HttpRequestMessageExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.Azure.EngagementFabric.Common;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities
{
    internal static class HttpRequestMessageExtension
    {
        public static string GetRequestId(this HttpRequestMessage request)
        {
            IEnumerable<string> values;
            if (request.Headers.TryGetValues(Constants.OperationTrackingIdHeader, out values))
            {
                return values.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }
    }
}
