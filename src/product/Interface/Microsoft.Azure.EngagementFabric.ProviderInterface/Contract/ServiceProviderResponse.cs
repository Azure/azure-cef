// -----------------------------------------------------------------------
// <copyright file="ServiceProviderResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.EngagementFabric.ProviderInterface.Contract
{
    [DataContract]
    public class ServiceProviderResponse
    {
        private const string JsonMediaType = "application/json";
        private const string TextMediaType = "text/plain";

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        [DataMember(Name = "StatusCode", Order = 1)]
        public HttpStatusCode StatusCode { get; set; }

        [DataMember(Name = "MediaType", Order = 2)]
        public string MediaType { get; set; }

        [DataMember(Name = "Content", Order = 3)]
        public string Content { get; set; }

        [DataMember(Name = "Headers", Order = 4)]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<IEnumerable<string>>))]
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; set; }

        public object JsonContent
        {
            set
            {
                this.MediaType = JsonMediaType;
                this.Content = JsonConvert.SerializeObject(value, Formatting.Indented, JsonSerializerSettings);
            }
        }

        public string StringContent
        {
            set
            {
                this.MediaType = TextMediaType;
                this.Content = value;
            }
        }

        public static ServiceProviderResponse CreateResponse(HttpStatusCode statusCode)
        {
            return new ServiceProviderResponse
            {
                StatusCode = statusCode
            };
        }

        public static ServiceProviderResponse CreateJsonResponse(HttpStatusCode statusCode, object jsonContent)
        {
            var response = CreateResponse(statusCode);
            response.JsonContent = jsonContent;
            return response;
        }

        public static ServiceProviderResponse CreateStringResponse(HttpStatusCode statusCode, string stringContent)
        {
            var response = CreateResponse(statusCode);
            response.StringContent = stringContent;
            return response;
        }
    }
}