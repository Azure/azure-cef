// <copyright file="PropertyCollectionExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Common.Extension
{
    public static class PropertyCollectionExtension
    {
        public static T Specialize<T>(this IDictionary<string, object> dict)
            where T : new()
        {
            var output = new T();

            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var attribute = propertyInfo
                    .GetCustomAttributes(typeof(JsonPropertyAttribute), false)
                    .OfType<JsonPropertyAttribute>()
                    .FirstOrDefault();

                var key = attribute?.PropertyName ?? propertyInfo.Name;

                object value;
                if (!dict.TryGetValue(key, out value))
                {
                    if (attribute.Required == Required.Always)
                    {
                        throw new ArgumentNullException(key, $"Missing required field '{key}'");
                    }

                    value = GetDefaultValue(propertyInfo.PropertyType);
                }
                else
                {
                    try
                    {
                        value = value?.CastTo(propertyInfo.PropertyType);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidCastException($"Failed to cast field '{key}' to {propertyInfo.PropertyType}", ex);
                    }
                }

                propertyInfo.SetValue(output, value);
            }

            return output;
        }

        private static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static object CastTo(this object value, Type type)
        {
            if (type.IsEnum)
            {
                return value is string ? Enum.Parse(type, (string)value) : Enum.ToObject(type, value);
            }

            if (type == typeof(Guid))
            {
                return Guid.Parse((string)value);
            }

            return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
        }
    }
}
