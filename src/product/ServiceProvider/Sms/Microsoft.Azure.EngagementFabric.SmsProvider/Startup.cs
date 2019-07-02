// -----------------------------------------------------------------------
// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common.Security;
using Microsoft.Azure.EngagementFabric.SmsProvider.Inbound;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Owin;

namespace Microsoft.Azure.EngagementFabric.SmsProvider
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName", Justification = "Keep using name of Startup")]
    public static class Startup
    {
        public static void ConfigureApp(IAppBuilder appBuilder, IInboundManager inboundManager)
        {
            // Configure Web API for self-host.
            var config = new HttpConfiguration();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());
            config.Formatters.JsonFormatter.UseDataContractJsonSerializer = false;
            config.MapHttpAttributeRoutes();

            config.Properties[typeof(IInboundManager).Name] = inboundManager;

            // TLS1.2+: Enable Connection Logging
            appBuilder.Use<ConnectionLogMiddleware>("Microsoft.Azure.EngagementFabric.SmsProvider");

            appBuilder.UseWebApi(config);
        }
    }
}