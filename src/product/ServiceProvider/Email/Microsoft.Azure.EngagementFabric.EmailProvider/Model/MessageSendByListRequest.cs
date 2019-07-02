// <copyright file="MessageSendByListRequest.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Microsoft.Azure.EngagementFabric.Common.Collection;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Model
{
    public class MessageSendByListRequest
    {
        [JsonProperty(PropertyName = "Emails", Required = Required.Always)]
        public List<string> Targets { get; set; }

        [JsonProperty(PropertyName = "MessageBody", Required = Required.Always)]
        public MessageTemplateBody MessageBody { get; set; }
    }
}
