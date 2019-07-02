// <copyright file="MessageSendRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class MessageSendRequest
    {
        [JsonProperty(PropertyName = "PhoneNumber", Required = Required.Always)]
        public List<string> Targets { get; set; }

        [JsonProperty(PropertyName = "Extend")]
        public string ExtendedCode { get; set; }

        [JsonProperty(PropertyName = "MessageBody", Required = Required.Always)]
        public MessageTemplateBody MessageBody { get; set; }

        public class MessageTemplateBody
        {
            [JsonProperty(PropertyName = "TemplateName", Required = Required.Always)]
            public string TemplateName { get; set; }

            [JsonProperty(PropertyName = "TemplateParam")]
            public PropertyCollection<string> TemplateParameters { get; set; }
        }
    }
}
