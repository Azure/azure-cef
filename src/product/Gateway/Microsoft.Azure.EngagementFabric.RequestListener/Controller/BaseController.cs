// <copyright file="BaseController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Versioning;
using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
using Microsoft.Azure.EngagementFabric.RequestListener.Common;
using Microsoft.Azure.EngagementFabric.RequestListener.Manager;

namespace Microsoft.Azure.EngagementFabric.RequestListener.Controller
{
    public class BaseController : ApiController
    {
        protected async Task<HttpResponseMessage> OnRequestAsync(string providerType)
        {
            var account = RequestHelper.ParseAccount(this.Request);
            var subscriptionId = await RequestHelper.GetSubscriptionId(account);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                // Get provider
                var provider = ProviderManager.GetServiceProvider(providerType);
                Validator.IsTrue<ArgumentException>(provider != null, nameof(providerType), "The provider type '{0}' is invalid.", providerType);

                var path = ParseServicePath(this.Request.RequestUri, providerType);

                // Build request
                var request = new ServiceProviderRequest
                {
                    HttpMethod = this.Request.Method.Method,
                    Path = path,
                    Content = await this.Request.Content.ReadAsStringAsync(),
                    Headers = this.Request.Headers.ToDictionary(pair => pair.Key, pair => pair.Value),
                    QueryNameValuePairs = this.Request.GetQueryNameValuePairs(),
                    // comment by jin: this is where apiVersion is validated
                    ApiVersion = ApiVersionHelper.GetApiVersion(this.Request.RequestUri)
                };

                // Dispatch
                var result = await provider.OnRequestAsync(request);

                // Build response
                var response = new HttpResponseMessage(result.StatusCode)
                {
                    Content = result.Content != null && result.MediaType != null
                        ? new StringContent(result.Content, Encoding.UTF8, result.MediaType)
                        : null
                };

                if (result.Headers != null && result.Headers.Count >= 0)
                {
                    foreach (var header in result.Headers)
                    {
                        response.Headers.Add(header.Key, header.Value);
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    MetricManager.Instance.LogRequestSuccess(1, account, subscriptionId, providerType);
                }

                return response;
            }
            catch
            {
                throw;
            }
            finally
            {
                stopwatch.Stop();
                MetricManager.Instance.LogRequestLatency(stopwatch.ElapsedMilliseconds, account, subscriptionId, providerType);
            }
        }

        private string ParseServicePath(Uri uri, string providerType)
        {
            var path = uri.AbsolutePath;
            var servicePath = $"/{providerType}/";
            return HttpUtility.UrlDecode(path.Substring(path.IndexOf(servicePath, StringComparison.OrdinalIgnoreCase) + servicePath.Length));
        }
    }
}
