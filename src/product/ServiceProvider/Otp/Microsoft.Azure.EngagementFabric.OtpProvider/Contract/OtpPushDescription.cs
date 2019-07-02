// <copyright file="OtpPushDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Contract
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Azure.EngagementFabric.Common.Collection;
    using Newtonsoft.Json;

    public class OtpPushDescription
    {
        [JsonProperty(PropertyName = "PhoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty(PropertyName = "Channel")]
        public string Channel { get; set; }

        // seconds for code expiration
        [JsonProperty(PropertyName = "ExpireTime")]
        public int? ExpireTime { get; set; }

        [JsonProperty(PropertyName = "CodeLength")]
        public int? CodeLength { get; set; }

        [JsonIgnore]
        public string PushType { get; set; }

        [JsonIgnore]
        public string PushMethod { get; set; }

        [JsonProperty(PropertyName = "TemplateName")]
        public string TemplateName { get; set; }

        [JsonProperty(PropertyName = "TemplateParam")]
        public PropertyCollection<string> TemplateParameters { get; set; }
    }
}
