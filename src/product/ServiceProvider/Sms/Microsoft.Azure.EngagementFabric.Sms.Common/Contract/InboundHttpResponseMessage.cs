// <copyright file="InboundHttpResponseMessage.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public class InboundHttpResponseMessage
    {
        public InboundHttpResponseMessage()
        {
        }

        public InboundHttpResponseMessage(int code)
        {
            this.HttpStatusCode = code;
        }

        [DataMember]
        public int HttpStatusCode { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string MediaType { get; set; }

        public HttpResponseMessage ToHttpResponseMessage()
        {
            var response = new HttpResponseMessage((HttpStatusCode)this.HttpStatusCode);
            if (!string.IsNullOrEmpty(this.Message))
            {
                response.Content = new StringContent(this.Message, Encoding.UTF8, this.MediaType);
            }

            return response;
        }
    }
}
