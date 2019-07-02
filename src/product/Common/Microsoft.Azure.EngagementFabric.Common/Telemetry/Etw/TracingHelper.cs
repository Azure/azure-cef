// <copyright file="TracingHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    public static class TracingHelper
    {
        public static string FormatTraceSource(object source, params object[] parameters)
        {
            // Allow callers to choose the prefix by passing a string instead of some other type
            if (source is string sourceString)
            {
                // Use the string provided.
            }
            else
            {
                // Allow passing in typeof(SomeClass) or an instance
                Type sourceType = source as Type ?? source.GetType();
                sourceString = GetFriendlyTypeName(sourceType);
            }

            if (parameters?.Length > 0)
            {
                return $"{sourceString}({string.Join(",", parameters)})";
            }
            else
            {
                return sourceString;
            }
        }

        private static string GetFriendlyTypeName(Type type)
        {
            var typeName = type.Name;
            if (typeName.Contains("`"))
            {
                typeName = typeName.Substring(0, typeName.IndexOf("`"));
            }

            if (type.IsNested)
            {
                typeName = type.DeclaringType.Name + "+" + typeName;
            }

            return typeName;
        }
    }
}
