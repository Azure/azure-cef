﻿// <copyright file="Program.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService
{
    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.
                ServiceRuntime.RegisterServiceAsync(
                    "Microsoft.Azure.EngagementFabric.ResourceProviderWebServiceType",
                    context => new ResourceProviderWebService(context))
                    .GetAwaiter()
                    .GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(ResourceProviderWebService).Name);

                // Prevents this host process from terminating so services keep running.
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }
    }
}
