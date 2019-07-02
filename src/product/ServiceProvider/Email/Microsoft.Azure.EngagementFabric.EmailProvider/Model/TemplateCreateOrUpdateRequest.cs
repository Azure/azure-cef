// <copyright file="TemplateCreateOrUpdateRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class TemplateCreateOrUpdateRequest
    {
        [JsonProperty(PropertyName = "TemplateName", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "SenderAddrID", Required = Required.Always)]
        public string SenderAddrID { get; set; }

        [JsonProperty(PropertyName = "SenderAlias", Required = Required.Always)]
        public string SenderAlias { get; set; }

        [JsonProperty(PropertyName = "Subject", Required = Required.Always)]
        public string Subject { get; set; }

        [JsonProperty(PropertyName = "HtmlMsg", Required = Required.Always)]
        public string HtmlMsg { get; set; }

        [JsonProperty(PropertyName = "EnableUnsubscribe")]
        public bool? EnableUnSubscribe { get; set; }
    }
}
