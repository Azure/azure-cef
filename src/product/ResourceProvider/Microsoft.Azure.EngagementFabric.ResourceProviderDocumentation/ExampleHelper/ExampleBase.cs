// <copyright file="ExampleBase.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.ExampleHelper
{
    public class ExampleBase : IExample
    {
        private static readonly JsonSerializerSettings SerializeSetting = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private static readonly JsonSerializer Serializer = new JsonSerializer
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public object Parameters { get; protected set; }

        public ResponseModel Response { get; protected set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(
                new SerializeModel
                {
                    Parameters = this.BuildParametersObject(),
                    Responses = this.BuildResponsesObject()
                },
                Formatting.Indented,
                SerializeSetting);
        }

        private JObject BuildParametersObject()
        {
            var obj = JObject.FromObject(
                this.GetType().GetProperties()
                    .Select(p => new KeyValuePair<string, object>(
                        p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName,
                        p.GetValue(this)))
                    .Where(pair => pair.Key != null && pair.Value != null)
                    .ToDictionary(pair => pair.Key, pair => new JValue(pair.Value)));

            if (this.Parameters != null)
            {
                obj.Merge(
                    JObject.FromObject(
                        this.Parameters,
                        Serializer));
            }

            return obj;
        }

        private JObject BuildResponsesObject()
        {
            return this.Response == null
                ? new JObject()
                : JObject.FromObject(
                    new Dictionary<string, object>
                    {
                        {
                            ((int)this.Response.StatusCode).ToString(),
                            this.Response
                        }
                    },
                    Serializer);
        }

        public class ResponseModel
        {
            [JsonIgnore]
            public HttpStatusCode StatusCode { get; set; }

            [JsonProperty("body")]
            public object Body { get; set; }
        }

        private class SerializeModel
        {
            [JsonProperty("parameters")]
            public object Parameters { get; set; }

            [JsonProperty("responses")]
            public object Responses { get; set; }
        }
    }
}
