// <copyright file="TenantController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Authorize;
using Microsoft.Azure.EngagementFabric.RequestListener.Common;
using Microsoft.Azure.EngagementFabric.TenantCache;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Controller
{
    [VersionFilter]
    public class TenantController : ApiController
    {
        [Route("tenants/{tenantName}")]
        [HttpGet]
        [CertificateBasedAuthorize]
        public async Task<HttpResponseMessage> GetAccountInfo(string tenantName)
        {
            var client = ReadOnlyTenantCacheClient.GetClient(true);
            var tenant = await client.GetTenantAsync(tenantName);
            Validator.IsTrue<ArgumentException>(tenant != null, nameof(tenant), "Tenant '{0}' does not exist.", tenantName);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(tenant), Encoding.UTF8, "application/json")
            };
        }
    }
}
