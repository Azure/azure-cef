// <copyright file="EmailController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common.Authorize;
using Microsoft.Azure.EngagementFabric.RequestListener.Common;
using Microsoft.Azure.EngagementFabric.RequestListener.Manager;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Controller
{
    [RoutePrefix("services/" + ProviderManager.EmailProviderType)]
    [VersionFilter]
    public class EmailController : BaseController
    {
        // Email provider supports two types of authentication
        // 1. Full only
        // 2. Admin
        // Will check #1 at last so that it can be used as default path which does not need to specify the paths
        [Route("admin/{*path}")]
        [AcceptVerbs("GET", "POST", "DELETE", "PUT")]
        [CertificateBasedAuthorize]
        public async Task<HttpResponseMessage> OnEmailRequestByInternalAsync(string path)
        {
            // TODO: ACIS certificate for admin AuthN
            return await this.OnRequestAsync(ProviderManager.EmailProviderType);
        }

        [Route("{*path}", Order = 99)]
        [AcceptVerbs("GET", "POST", "DELETE", "PUT")]
        [SASAuthorize(Roles = "full")]
        public async Task<HttpResponseMessage> OnEmailRequestByClientAsync(string path)
        {
            return await this.OnRequestAsync(ProviderManager.EmailProviderType);
        }
    }
}
