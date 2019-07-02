// <copyright file="RestrictedStringEnumConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities
{
    /// <summary>
    /// The converter which will throw exception on deserializing unknown menu values
    /// </summary>
    public class RestrictedStringEnumConverter : StringEnumConverter
    {
        /// <summary>
        /// Reads the JSON representation of the object
        /// </summary>
        /// <param name="reader">The Newtonsoft.Json.JsonReader to read from</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="existingValue">The existing value of object being read</param>
        /// <param name="serializer">The calling serializer</param>
        /// <returns>The object value</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var enumText = reader.Value.ToString();
            return Enum.Parse(objectType, enumText, true);
        }
    }
}
