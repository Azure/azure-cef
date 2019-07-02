// <copyright file="CertificateBasedAuthorizeAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using System.Web.Http.Controllers;
using Microsoft.Azure.EngagementFabric.Common.Extension;

namespace Microsoft.Azure.EngagementFabric.Common.Authorize
{
    public class CertificateBasedAuthorizeAttribute : AuthorizeAttribute
    {
        public const string ValidClientCertificateKey = "CertificateBasedAuthorizeAttribute.ValidClientCertificateAsync";
        public const string AuthenticationFailureHandlerKey = "CertificateBasedAuthorizeAttribute.TraceException";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                var validator = actionContext.GetControllerConfiguration<Func<X509Certificate2, bool>>(ValidClientCertificateKey);

                if (actionContext.RequestContext.ClientCertificate == null)
                {
                    throw new UnauthorizedAccessException("No client certificate");
                }
                else if (validator == null)
                {
                    throw new UnauthorizedAccessException("No validator");
                }
                else if (!validator(actionContext.RequestContext.ClientCertificate))
                {
                    throw new UnauthorizedAccessException("Invalid certificate");
                }
            }
            catch (Exception ex)
            {
                actionContext.GetControllerConfiguration<Action<Exception>>(AuthenticationFailureHandlerKey)?.Invoke(ex);
                throw;
            }
        }
    }
}
