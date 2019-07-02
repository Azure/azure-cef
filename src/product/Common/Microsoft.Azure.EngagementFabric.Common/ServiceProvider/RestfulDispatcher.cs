// <copyright file="RestfulDispatcher.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using Microsoft.Azure.EngagementFabric.Common.ParameterBind;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Common.ServiceProvider
{
    public class RestfulDispatcher<TResponseModel>
    {
        private readonly Action<string> traceMessage;
        private readonly object controller;
        private readonly RouteTableEntry[] routeTable;

        public RestfulDispatcher(Action<string> traceMessage, object controller)
        {
            this.traceMessage = traceMessage;
            this.controller = controller;

            this.routeTable = controller.GetType()
                .GetMethods()
                .Where(methodInfo => methodInfo.ReturnType == typeof(Task<TResponseModel>))
                .Select(methodInfo => RouteTableEntry.Create(methodInfo))
                .Where(entry => entry != null)
                .ToArray();
        }

        public async Task<TResponseModel> DispatchAsync(
            string httpMethod,
            string path,
            string content,
            IReadOnlyDictionary<string, IEnumerable<string>> headers,
            IEnumerable<KeyValuePair<string, string>> query)
        {
            RouteTableEntry selectedEntry = null;
            Dictionary<string, string> pathVariables = null;

            foreach (var entry in this.routeTable)
            {
                pathVariables = entry.GetPathVariables(httpMethod, path);
                if (pathVariables != null)
                {
                    selectedEntry = entry;
                    break;
                }
            }

            if (selectedEntry == null)
            {
#if DEBUG
                throw new ResourceNotFoundException($"Invalid path '{path}' for resource provider {this.controller.GetType().Name}");
#else
                throw new ResourceNotFoundException($"Invalid path '{path}'");
#endif
            }

            var method = this.controller.GetType().GetMethod(selectedEntry.Action, selectedEntry.ParameterTypes);

            var parameters = method.GetParameters()
                .Select(parameterInfo => CastParameter(
                    parameterInfo,
                    content,
                    pathVariables,
                    headers,
                    query))
                .ToArray();

            var task = method.Invoke(this.controller, parameters.ToArray()) as Task<TResponseModel>;
            return await task;
        }

        private static object CastParameter(
            ParameterInfo parameterInfo,
            string content,
            Dictionary<string, string> pathVariables,
            IReadOnlyDictionary<string, IEnumerable<string>> headers,
            IEnumerable<KeyValuePair<string, string>> query)
        {
            if (parameterInfo.GetCustomAttributes(true).OfType<FromBodyAttribute>().Any())
            {
                try
                {
                    if (parameterInfo.ParameterType == typeof(string))
                    {
                        return content;
                    }
                    else if (parameterInfo.ParameterType == typeof(XDocument))
                    {
                        return XDocument.Parse(content);
                    }
                    else
                    {
                        return JsonConvert.DeserializeObject(content, parameterInfo.ParameterType);
                    }
                }
                catch
                {
                    throw new HttpRequestInvalidParameterTypeException(parameterInfo.Name, parameterInfo.ParameterType);
                }
            }

            var headerAttribute = parameterInfo.GetCustomAttributes(true).OfType<FromHeaderAttribute>().FirstOrDefault();
            if (headerAttribute != null)
            {
                IEnumerable<string> headerValues;
                if (headers != null && headers.TryGetValue(headerAttribute.HeaderName ?? parameterInfo.Name, out headerValues))
                {
                    try
                    {
                        return ChangeType(headerValues.First(), parameterInfo.ParameterType);
                    }
                    catch
                    {
                        throw new HttpRequestInvalidParameterTypeException(parameterInfo.Name, parameterInfo.ParameterType);
                    }
                }
                else
                {
                    throw new HttpRequestMissingParameterException(parameterInfo.Name);
                }
            }

            var queryAttribute = parameterInfo.GetCustomAttributes(true).OfType<FromQueryAttribute>().SingleOrDefault();
            if (queryAttribute != null)
            {
                var queryValue = query?.FirstOrDefault(pair => pair.Key.Equals(queryAttribute.QueryName ?? parameterInfo.Name, StringComparison.OrdinalIgnoreCase)).Value;
                if (!string.IsNullOrEmpty(queryValue))
                {
                    try
                    {
                        return ChangeType(queryValue, parameterInfo.ParameterType);
                    }
                    catch
                    {
                        throw new HttpRequestInvalidParameterTypeException(parameterInfo.Name, parameterInfo.ParameterType);
                    }
                }
                else if (!parameterInfo.IsOptional)
                {
                    throw new HttpRequestMissingParameterException(parameterInfo.Name);
                }
                else
                {
                    return parameterInfo.DefaultValue;
                }
            }

            string pathValue;
            if (pathVariables.TryGetValue(parameterInfo.Name, out pathValue))
            {
                try
                {
                    return ChangeType(pathValue, parameterInfo.ParameterType);
                }
                catch
                {
                    throw new HttpRequestInvalidParameterTypeException(parameterInfo.Name, parameterInfo.ParameterType);
                }
            }

            throw new HttpRequestMissingParameterException(parameterInfo.Name);
        }

        private static object ChangeType(string input, Type type)
        {
            if (type == typeof(Guid))
            {
                return Guid.Parse(input);
            }
            else
            {
                return Convert.ChangeType(input, type);
            }
        }

        private class RouteTableEntry
        {
            private static readonly Dictionary<Type, string> HttpMethodMapping = new Dictionary<Type, string>
            {
                { typeof(HttpPostAttribute),    "POST" },
                { typeof(HttpGetAttribute),     "GET" },
                { typeof(HttpPutAttribute),     "PUT" },
                { typeof(HttpDeleteAttribute),  "DELETE" },
            };

            public string Action { get; set; }

            public Type[] ParameterTypes { get; set; }

            public string HttpMethod { get; set; }

            public string[] Parts { get; set; }

            public static RouteTableEntry Create(MethodInfo methodInfo)
            {
                var attributes = methodInfo.GetCustomAttributes(true);

                var routeAttribute = attributes.OfType<RouteAttribute>().SingleOrDefault();
                if (routeAttribute == null)
                {
                    return null;
                }

                foreach (var pair in HttpMethodMapping)
                {
                    if (attributes.Any(a => a.GetType() == pair.Key))
                    {
                        return new RouteTableEntry
                        {
                            Action = methodInfo.Name,
                            ParameterTypes = methodInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray(),
                            HttpMethod = pair.Value,
                            Parts = routeAttribute.Template.Split('/')
                        };
                    }
                }

                return null;
            }

            public Dictionary<string, string> GetPathVariables(string httpMethod, string path)
            {
                if (this.HttpMethod != httpMethod)
                {
                    return null;
                }

                var parts = path.Split('/');
                if (this.Parts.Length != parts.Length)
                {
                    return null;
                }

                var pairs = this.Parts.Zip(parts, (expected, actual) => new KeyValuePair<string, string>(expected, actual));
                var groups = pairs.GroupBy(pair => IsVariable(pair.Key));

                var fixedPairs = groups.SingleOrDefault(g => !g.Key)?.AsEnumerable() ?? new KeyValuePair<string, string>[] { };
                var variablePairs = groups.SingleOrDefault(g => g.Key)?.AsEnumerable() ?? new KeyValuePair<string, string>[] { };

                if (fixedPairs.Any(pair => !string.Equals(pair.Key, pair.Value, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return null;
                }

                return variablePairs.ToDictionary(
                    pair => GetVariableName(pair.Key),
                    pair => pair.Value,
                    StringComparer.InvariantCultureIgnoreCase);
            }

            private static bool IsVariable(string input)
            {
                return input.StartsWith("{") && input.EndsWith("}");
            }

            private static string GetVariableName(string input)
            {
                return input.Substring(1, input.Length - 2);
            }
        }
    }
}
