// <copyright file="MessageTemplateBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Contract
{
    public class MessageTemplateBody
    {
        [JsonProperty(PropertyName = "TemplateName", Required = Required.Always)]
        public string TemplateName { get; set; }

        [JsonProperty(PropertyName = "TemplateParam")]
        public PropertyCollection<string> TemplateParameters { get; set; }
    }
}
