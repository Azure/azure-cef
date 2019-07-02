// <copyright file="TemplateCreateOrUpdateRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class TemplateCreateOrUpdateRequest
    {
        [JsonProperty(PropertyName = "TemplateName", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "Signature", Required = Required.Always)]
        public string Signature { get; set; }

        [JsonProperty(PropertyName = "TplType", Required = Required.Always)]
        public MessageCategory Category { get; set; }

        [JsonProperty(PropertyName = "Message", Required = Required.Always)]
        public string Body { get; set; }
    }
}
