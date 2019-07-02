// -----------------------------------------------------------------------
// <copyright file="WebApiConfig.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Formatting;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Authorize;
using Microsoft.Azure.EngagementFabric.Common.Pagination;
using Microsoft.Azure.EngagementFabric.Common.Security;
using Microsoft.Azure.EngagementFabric.RequestListener.Common;
using Microsoft.Azure.EngagementFabric.RequestListener.Manager;
using Microsoft.Azure.EngagementFabric.RequestListener.Store;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using Swashbuckle.Application;

namespace Microsoft.Azure.EngagementFabric.RequestListener
{
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:FileHeaderFileNameDocumentationMustMatchTypeName", Justification = "Keep using name of Startup")]
    public static class Startup
    {
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host.
            var config = new HttpConfiguration();

            // Configure format
            var jsonFormatter = new JsonMediaTypeFormatter();

            jsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Formatters.Clear();
            config.Formatters.Insert(0, jsonFormatter);

            config.MapHttpAttributeRoutes();

            // SAS Authentication
            var factory = new AdminStoreFactory();
            var store = factory.GetStore();
            config.Properties[SASAuthorizeAttribute.ConfigurationPropertyKey] = new SASAuthorizeAttribute.GetKeysAsyncFunc(
                async (accountName, keyNames) => await store.GetKeysAsync(accountName, keyNames));

            config.Properties[SASAuthorizeAttribute.AuthenticationFailureHandlerKey] = new SASAuthorizeAttribute.OnAuthenticationFailed(
                async (exception, request) =>
                {
                    var account = RequestHelper.ParseAccount(request);
                    var subscriptionId = await RequestHelper.GetSubscriptionId(account);

                    MetricManager.Instance.LogRequestFailed4xx(1, account, subscriptionId, string.Empty);
                });

            config.Services.Replace(typeof(IExceptionHandler), new CustomExceptionHandler());
            config.MessageHandlers.Add(new ApiTrackHandler());

            // Certificate Authentication
            config.Properties[CertificateBasedAuthorizeAttribute.ValidClientCertificateKey] = new Func<X509Certificate2, bool>(
                (cert) => CertificateHelper.ValidClientCertificate(cert));

            config.Properties[CertificateBasedAuthorizeAttribute.AuthenticationFailureHandlerKey] = new Action<Exception>(
                (exception) => MetricManager.Instance.LogRequestFailed4xx(1, "Acis", "Acis", string.Empty));

            // Temp for ibiza extension
            var exposeHeaders = new List<string>
            {
                Constants.OperationTrackingIdHeader,
                ContinuationToken.ContinuationTokenKey
            };
            var cors = new EnableCorsAttribute("*", "*", "*", string.Join(",", exposeHeaders));
            config.EnableCors(cors);

            // Publish static content
            var physicalFileSystem = new PhysicalFileSystem(@"./StaticContent");
            var options = new FileServerOptions
            {
                EnableDefaultFiles = true,
                FileSystem = physicalFileSystem,
            };

            options.StaticFileOptions.FileSystem = physicalFileSystem;
            options.StaticFileOptions.ServeUnknownFileTypes = true;
            appBuilder.UseFileServer(options);

            // Enable Swagger UI
            var swaggerEnabledConfig = new SwaggerEnabledConfiguration(config, SwaggerDocsConfig.DefaultRootUrlResolver, new[] { "Swagger/2018-10-01.json" });
            swaggerEnabledConfig.EnableSwaggerUi();

            // TLS1.2+: Enable Connection Logging
            appBuilder.Use<ConnectionLogMiddleware>("Microsoft.Azure.EngagementFabric.RequestListener");

            appBuilder.UseWebApi(config);

            config.EnsureInitialized();
        }
    }
}