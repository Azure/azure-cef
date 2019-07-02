// <copyright file="InboundHttpRequestMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.Web;
using Microsoft.Owin;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public class InboundHttpRequestMessage
    {
        public InboundHttpRequestMessage()
        {
        }

        public InboundHttpRequestMessage(HttpRequestMessage message)
        {
            this.RequestUri = message.RequestUri.AbsolutePath;
            this.CallerIp = GetCallerIp(message);
            this.Content = message.Content.ReadAsByteArrayAsync().Result;
            this.Headers = message.Headers.ToDictionary(a => a.Key, a => a.Value.ToList());
        }

        [DataMember]
        public string RequestUri { get; set; }

        [DataMember]
        public string CallerIp { get; set; }

        [DataMember]
        public byte[] Content { get; set; }

        [DataMember]
        public Dictionary<string, List<string>> Headers { get; set; }

        private string GetCallerIp(HttpRequestMessage request)
        {
            if (request == null)
            {
                return null;
            }

            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }

            if (request.Properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                return ((RemoteEndpointMessageProperty)request.Properties[RemoteEndpointMessageProperty.Name]).Address;
            }

            if (request.Properties.ContainsKey("MS_OwinContext"))
            {
                return ((OwinContext)request.Properties["MS_OwinContext"]).Request.RemoteIpAddress;
            }

            return null;
        }
    }
}
