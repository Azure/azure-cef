// <copyright file="DateTimeOffsetJsonConverter.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Common.Json
{
    public class DateTimeOffsetJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTimeOffset)
            {
                writer.WriteValue(((DateTimeOffset)value).ToString("o"));
            }
        }
    }
}
