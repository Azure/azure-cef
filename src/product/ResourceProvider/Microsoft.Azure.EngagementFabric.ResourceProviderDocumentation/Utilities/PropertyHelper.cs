// <copyright file="PropertyHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Utilities
{
    internal static class PropertyHelper
    {
        public static PropertyInfo GetProperty(Type type, string name)
        {
            return type.GetProperties()
                .FirstOrDefault(p =>
                    string.Equals(p.Name, name) ||
                    string.Equals(p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName, name));
        }
    }
}
