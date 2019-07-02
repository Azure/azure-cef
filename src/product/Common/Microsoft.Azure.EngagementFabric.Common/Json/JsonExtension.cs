// -----------------------------------------------------------------------
// <copyright file="JsonExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Common.Json
{
    public static class JsonExtension
    {
        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.None);
        }

        public static TObject FromJson<TObject>(this string value)
        {
            return JsonConvert.DeserializeObject<TObject>(value);
        }
    }
}
