// <copyright file="OtpController.cs" company="Microsoft Corporation">
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
    [RoutePrefix("services/" + ProviderManager.OtpProviderType)]
    [VersionFilter]
    public class OtpController : BaseController
    {
        // OTP provider supports only one types of authentication
        // 1. Device and Full
        [Route("{*path}", Order = 99)]
        [AcceptVerbs("GET", "POST", "DELETE", "PUT")]
        [SASAuthorize(Roles = "full;device")]
        public async Task<HttpResponseMessage> OnOtpRequestByClientAndDeviceAsync(string path)
        {
            return await this.OnRequestAsync(ProviderManager.OtpProviderType);
        }
    }
}
