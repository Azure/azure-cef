// -----------------------------------------------------------------------
// <copyright file="ServiceProviderRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common.Json;
using Microsoft.Azure.EngagementFabric.Common.Versioning;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ProviderInterface.Contract
{
    [DataContract]
    public class ServiceProviderRequest
    {
        [DataMember(Name = "HttpMethod", Order = 1)]
        public string HttpMethod { get; set; }

        [DataMember(Name = "Path", Order = 2)]
        public string Path { get; set; }

        [DataMember(Name = "Content", Order = 3)]
        public string Content { get; set; }

        [DataMember(Name = "Headers", Order = 4)]
        [JsonConverter(typeof(CaseInsensitiveDictionaryConverter<IEnumerable<string>>))]
        public IReadOnlyDictionary<string, IEnumerable<string>> Headers { get; set; }

        [DataMember(Name = "QueryNameValuePairs", Order = 5)]
        public IEnumerable<KeyValuePair<string, string>> QueryNameValuePairs { get; set; }

        [DataMember(Name = "ApiVersion", Order = 6)]
        public ApiVersion ApiVersion { get; set; }
    }
}