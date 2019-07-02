// <copyright file="OwinCommunicationListener.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Fabric;
using System.Fabric.Description;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Owin.Hosting;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Owin;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService
{
    internal class OwinCommunicationListener : ICommunicationListener
    {
        private readonly Action<IAppBuilder, StatelessServiceContext> startup;
        private readonly StatelessServiceContext serviceContext;
        private readonly EndpointResourceDescription endpoint;

        private IDisposable webApp;

        public OwinCommunicationListener(
            Action<IAppBuilder, StatelessServiceContext> startup,
            StatelessServiceContext serviceContext,
            EndpointResourceDescription endpoint)
        {
            this.startup = startup;
            this.serviceContext = serviceContext;
            this.endpoint = endpoint;
        }

        public async Task<string> OpenAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            var listeningAddress = $"{this.endpoint.Protocol}://+:{this.endpoint.Port}";
            var publishAddress = listeningAddress.Replace("+", FabricRuntime.GetNodeContext().IPAddressOrFQDN);

            try
            {
                this.Log($"Starting web server on {listeningAddress}");
                this.webApp = WebApp.Start(listeningAddress, appBuilder => this.startup(appBuilder, this.serviceContext));
                this.Log($"Listening on {publishAddress}");
                return publishAddress;
            }
            catch (Exception ex)
            {
                this.Log($"Web server failed to open endpoint {this.endpoint.Name}. {ex}");
                this.StopWebServer();
                throw;
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;

            this.Log($"Closing web server on endpoint {this.endpoint.Name}");
            this.StopWebServer();
        }

        public void Abort()
        {
            this.Log($"Aborting web server on endpoint {this.endpoint.Name}");
            this.StopWebServer();
        }

        private void StopWebServer()
        {
            if (this.webApp == null)
            {
                return;
            }

            try
            {
                this.webApp.Dispose();
            }
            catch (ObjectDisposedException)
            {
            }
        }

        private void Log(string message)
        {
            ServiceEventSource.Current.ServiceMessage(this.serviceContext, message);
        }
    }
}
