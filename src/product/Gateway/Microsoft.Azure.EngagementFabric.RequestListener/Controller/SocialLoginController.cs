// <copyright file="SocialLoginController.cs" company="Microsoft Corporation">
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
    [RoutePrefix("services/" + ProviderManager.SocialProviderType)]
    [VersionFilter]
    public class SocialLoginController : BaseController
    {
        // Social login provider supports two types of authentication
        // 1. Device and Full
        // 2. Full only
        // Will check #1 at last so that it can be used as default path which does not need to specify the paths
        [Route("userinfo")]
        [AcceptVerbs("DELETE")]
        [SASAuthorize(Roles = "full")]
        public async Task<HttpResponseMessage> OnSocialLoginRequestByClientOnlyAsync()
        {
            return await this.OnRequestAsync(ProviderManager.SocialProviderType);
        }

        [Route("{*path}", Order = 99)]
        [AcceptVerbs("GET", "POST", "DELETE", "PUT")]
        [SASAuthorize(Roles = "full;device")]
        public async Task<HttpResponseMessage> OnSocialLoginRequestByClientAndDeviceAsync(string path)
        {
            return await this.OnRequestAsync(ProviderManager.SocialProviderType);
        }
    }
}
