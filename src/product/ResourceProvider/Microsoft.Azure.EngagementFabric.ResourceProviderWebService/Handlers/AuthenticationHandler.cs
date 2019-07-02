// <copyright file="AuthenticationHandler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Utilities;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Handlers
{
    internal class AuthenticationHandler : DelegatingHandler
    {
        private static readonly IReadOnlyDictionary<string, string> MetadataEndpoints = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "AzureCloud",
                "https://management.azure.com:24582/metadata/authentication?api-version=2015-01-01"
            },
            {
                "Dogfood",
                "https://api-dogfood.resources.windows-int.net:24582/metadata/authentication?api-version=2015-01-01"
            },
            {
                "AzureChinaCloud",
                "https://management.chinacloudapi.cn:24582/metadata/authentication?api-version=2015-01-01"
            }
        };

        private static readonly TimeSpan UpdateInterval = TimeSpan.FromDays(1);
        private static readonly TimeSpan UpdateRetryInterval = TimeSpan.FromMinutes(5);

        private readonly StatelessServiceContext serviceContext;
        private readonly string metadataEndpoint;
        private DateTime lastClientCertificateUpdate;
        private IEnumerable<ClientCertificateDescription> clientCertificates;

        public AuthenticationHandler(StatelessServiceContext serviceContext)
        {
            this.serviceContext = serviceContext;

            var azureEnvironmentName = this.serviceContext
                .CodePackageActivationContext
                .GetConfig<string>("ResourceProviderWebService", "AzureEnvironmentName");

            this.metadataEndpoint = null;
            this.lastClientCertificateUpdate = DateTime.MinValue;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (this.metadataEndpoint != null)
            {
                var utcNow = DateTime.UtcNow;
                if (utcNow > this.lastClientCertificateUpdate + UpdateInterval)
                {
                    try
                    {
                        this.clientCertificates = await this.UpdateClientCertificatesAsync();

                        this.lastClientCertificateUpdate = utcNow;
                        ResourceProviderEventSource.Current.Info(
                            request.GetRequestId() ?? "n/a",
                            this,
                            nameof(SendAsync),
                            OperationStates.Set,
                            "Acceptable client certificate list updated");
                    }
                    catch (Exception ex)
                    {
                        this.lastClientCertificateUpdate += UpdateRetryInterval;
                        ResourceProviderEventSource.Current.ErrorException(
                            request.GetRequestId() ?? "n/a",
                            this,
                            nameof(SendAsync),
                            OperationStates.Failed,
                            "Failed to update the acceptable client certificate list",
                            ex);
                    }
                }
                var cert = request.GetClientCertificate();
                if (cert == null)
                {
                    ResourceProviderEventSource.Current.Warning(
                        request.GetRequestId() ?? "n/a",
                        this,
                        nameof(SendAsync),
                        OperationStates.Dropped,
                        "Rejected due to absent of client certificate");

                    return request.CreateResponse(HttpStatusCode.Forbidden);
                }

                var authenticated = this.clientCertificates
                    .Where(c => utcNow >= c.NotBefore && utcNow <= c.NotAfter)
                    .Any(c => string.Equals(c.Thumbprint, cert.Thumbprint, StringComparison.OrdinalIgnoreCase));

                if (!authenticated)
                {  
                    // updated by jin
                    // post-fix(todo) 
                    //ResourceProviderEventSource.Current.Warning(
                    //    request.GetRequestId() ?? "n/a",
                    //    this,
                    //    nameof(SendAsync),
                    //    OperationStates.Dropped,
                    //    "Rejected due to absent of matching client certificate");

                    //return request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }

        private async Task<IEnumerable<ClientCertificateDescription>> UpdateClientCertificatesAsync()
        {
            using (var client = new HttpClient())
            {
                var content = await client.GetStringAsync(this.metadataEndpoint);

                var list = JsonConvert.DeserializeObject<ClientCertificateDescriptionList>(content);
                return list.ClientCertificates;
            }
        }
    }
}
