// <copyright file="Startup.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Fabric;
using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Microsoft.Azure.EngagementFabric.Common.Extension;
using Microsoft.Azure.EngagementFabric.Common.Security;
using Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Filters;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Controllers;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Handlers;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Managers;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Metadata;
using Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;
using Swashbuckle.Application;
using Swashbuckle.Swagger;
using Unity;
using Unity.AspNet.WebApi;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService
{
    internal static class Startup
    {
        public static void ConfigureApp(
            IAppBuilder appBuilder,
            StatelessServiceContext serviceContext)
        {
            ConfigureSkuStore(serviceContext);

            var config = new HttpConfiguration();
            config.Properties[BaseController.ServiceContextKey] = serviceContext;

            ConfigureHttp(config, serviceContext);
            ConfigureUnity(config, serviceContext);
            ConfigureSwagger(config);

            // TLS1.2+: Enable Connection Logging
            appBuilder.Use<ConnectionLogMiddleware>("Microsoft.Azure.EngagementFabric.ResourceProviderWebService");

            appBuilder.UseWebApi(config);
        }

        private static void ConfigureSkuStore(StatelessServiceContext serviceContext)
        {
            var serializedLocations = serviceContext
                .CodePackageActivationContext
                .GetConfig<string>("ResourceProviderWebService", "Locations");
            var serializedSKUs = serviceContext
                .CodePackageActivationContext
                .GetConfig<string>("ResourceProviderWebService", "SKUs");

            IEnumerable<string> locations;
            try
            {
                locations = JsonConvert.DeserializeObject<IEnumerable<string>>(serializedLocations);
            }
            catch
            {
                locations = new string[] { string.Empty };
            }

            IEnumerable<string> skus;
            try
            {
                skus = JsonConvert.DeserializeObject<IEnumerable<string>>(serializedSKUs);
            }
            catch
            {
                skus = new string[] { string.Empty };
            }

            SkuStore.Initialize(locations, skus);
        }

        private static void ConfigureHttp(
            HttpConfiguration config,
            StatelessServiceContext serviceContext)
        {
            var exceptionHandler = new CustomExceptionHandler(serviceContext);
            config.Services.Replace(typeof(IExceptionHandler), exceptionHandler);

            var authenticationHandler = new AuthenticationHandler(serviceContext);
            config.MessageHandlers.Add(authenticationHandler);

            var traceHandler = new TraceHandler(serviceContext);
            config.MessageHandlers.Add(traceHandler);

            var serializer = config.Formatters.JsonFormatter;
            serializer.SerializerSettings.Formatting = Formatting.Indented;
            serializer.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            config.Formatters.Clear();
            var jsonFormatter = new JsonMediaTypeFormatter();
            jsonFormatter.SupportedMediaTypes.Remove(new MediaTypeHeaderValue("text/json"));
            config.Formatters.Add(jsonFormatter);

            config.MapHttpAttributeRoutes();
        }

        private static void ConfigureUnity(
            HttpConfiguration config,
            StatelessServiceContext serviceContext)
        {
            var defaultConnectionString = serviceContext
                .CodePackageActivationContext
                .GetConfig<string>("ResourceProviderWebService", "DefaultConnectionString");

            var store = new ResourceProviderStore(defaultConnectionString);

            UnityConfig.Container.RegisterInstance<IResourceProviderStore>(store);
            UnityConfig.Container.RegisterSingleton<IAccountManager, AccountManager>();
            UnityConfig.Container.RegisterSingleton<ISubscriptionManager, SubscriptionManager>();
            config.DependencyResolver = new UnityDependencyResolver(UnityConfig.Container);
        }

        private static void ConfigureSwagger(HttpConfiguration config)
        {
            var globalParameterFilter = new GlobalParameterFilter(
                new Dictionary<string, Parameter>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {
                        "SubscriptionIdParameter",
                        new Parameter
                        {
                            name = "subscriptionId",
                            @in = "path",
                            required = true,
                            type = "string",
                            description = "Subscription ID"
                        }
                    },
                    {
                        "ResourceGroupNameParameter",
                        new Parameter
                        {
                            name = "resourceGroupName",
                            @in = "path",
                            required = true,
                            type = "string",
                            description = "Resource Group Name",
                            vendorExtensions = new Dictionary<string, object>
                            {
                                {
                                    "x-ms-parameter-location", "method"
                                }
                            }
                        }
                    },
                    {
                        "AccountNameParameter",
                        new Parameter
                        {
                            name = "accountName",
                            @in = "path",
                            required = true,
                            type = "string",
                            description = "Account Name",
                            vendorExtensions = new Dictionary<string, object>
                            {
                                {
                                    "x-ms-parameter-location", "method"
                                }
                            }
                        }
                    },
                    {
                        "ChannelNameParameter",
                        new Parameter
                        {
                            name = "channelName",
                            @in = "path",
                            required = true,
                            type = "string",
                            description = "Channel Name",
                            vendorExtensions = new Dictionary<string, object>
                            {
                                {
                                    "x-ms-parameter-location", "method"
                                }
                            }
                        }
                    },
                    {
                        "ApiVersionParameter",
                        new Parameter
                        {
                            name = "api-version",
                            @in = "query",
                            required = true,
                            type = "string",
                            description = "API version"
                        }
                    }
                });

            config.EnableSwagger(c =>
            {
                c.PrettyPrint();

                c.Schemes(new[] { "https" });

                c.SingleApiVersion(ApiVersionStore.DefaultApiVersion, NameStore.ServiceTitle)
                    .Description(NameStore.ServiceDescription);

                c.OAuth2("azure_auth")
                    .AuthorizationUrl("https://login.microsoftonline.com/common/oauth2/authorize")
                    .Flow("implicit")
                    .Description("Azure Active Directory OAuth2 Flow")
                    .Scopes(s => s.Add("user_impersonation", "impersonate your user account"));

                var commentFile = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
                c.IncludeXmlComments(commentFile);

                var enumFilter = new EnumFilter(commentFile);

                c.SchemaFilter<SubclassFilter>();
                c.SchemaFilter<AzureResourceFilter>();
                c.SchemaFilter<ClientFlattenFilter>();
                c.SchemaFilter(() => enumFilter);
                c.SchemaFilter<MutabilityFilter>();
                c.SchemaFilter<ReadOnlyFilter>();
                c.SchemaFilter<ExternalFilter>();

                c.OperationFilter(() => globalParameterFilter);
                c.OperationFilter<DefaultResponseFilter>();
                c.OperationFilter<ExampleFilter>();
                c.OperationFilter<NonPageableFilter>();
                c.OperationFilter<DocumentFilter>();

                c.DocumentFilter(() => enumFilter);
                c.DocumentFilter<SubclassFilter>();
                c.DocumentFilter(() => globalParameterFilter);
                c.DocumentFilter<DocumentFilter>();
            })
            .EnableSwaggerUi();
        }
    }
}
