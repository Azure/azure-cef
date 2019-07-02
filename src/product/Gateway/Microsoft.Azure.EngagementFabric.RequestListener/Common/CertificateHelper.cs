// <copyright file="CertificateHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Common
{
    public static class CertificateHelper
    {
        public static bool ValidClientCertificate(X509Certificate2 certificate)
        {
            // We will only accept the certificate as a valid certificate if all the conditions below are met:
            // 1. The certificate is not expired and is active for the current time on server.
            // 2. The thumbprint of the certificate matches one of the known thumbprints.
            if (certificate == null || certificate.Thumbprint == null)
            {
                GatewayEventSource.Current.Warning(GatewayEventSource.EmptyTrackingId, "CertificateHelper", "ValidClientCertificateAsync", OperationStates.Empty, "Failed due to null cert or null thumbprint");
                return false;
            }

            // Check known thumbprints (Acis)
            var thumbprint = certificate.Thumbprint.Trim();
            if (!string.IsNullOrEmpty(RequestListenerService.ServiceConfiguration.AcisCertificateThumbprint) &&
                !string.Equals(RequestListenerService.ServiceConfiguration.AcisCertificateThumbprint, thumbprint, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Check that the certificate hasn't expired and is valid
            if (DateTime.Compare(DateTime.Now, certificate.NotBefore) < 0 ||
                DateTime.Compare(DateTime.Now, certificate.NotAfter) > 0)
            {
                GatewayEventSource.Current.Warning(GatewayEventSource.EmptyTrackingId, "CertificateHelper", "ValidClientCertificateAsync", OperationStates.Empty, "Failed due to expired cert");
                return false;
            }

            return true;
        }
    }
}
