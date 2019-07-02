 //<copyright file = "AuditClient.cs" company="Microsoft Corporation">
 //Copyright(c) Microsoft Corporation.All rights reserved.
 //</copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Authorize;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Common.Security
{
    public class AuditClient
    {
        private static readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage, Task>> AuditActions = new Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage, Task>>
        {
            { "^/services/sms/signatures(/?)(.*)$", AuditForSmsSignature },
            { "^/services/sms/templates(/?)(.*)$", AuditForSmsTemplate }
        };

        public AuditClient(string provider)
        {
            //if (IfxInitializer.IfxInitializeStatus == IfxInitializer.IfxInitState.IfxUninitalized)
            //{
            //    var sessionName = $"{provider}IfxSession";
            //    IfxInitializer.Initialize(sessionName);
            //}
        }

        public async Task AuditIfPrivileged(HttpRequestMessage request, HttpResponseMessage response)
        {
            request.Headers.TryGetValues(Constants.OperationTrackingIdHeader, out IEnumerable<string> values);
            var requestId = values?.FirstOrDefault();

            try
            {
                foreach (var action in AuditActions)
                {
                    var regex = new Regex(action.Key);
                    if (regex.IsMatch(request.RequestUri.AbsolutePath.ToLower()))
                    {
                        await action.Value(request, response);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                SecurityEventSource.Current.TraceAuditException(requestId ?? SecurityEventSource.EmptyTrackingId, request.RequestUri.AbsolutePath, ex.ToString());
            }
        }

        private static async Task AuditForSmsSignature(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (request.Method == HttpMethod.Delete)
            {
                await LogApplicationAudit("SmsSignatureDelete", request, response);
            }
        }

        private static async Task AuditForSmsTemplate(HttpRequestMessage request, HttpResponseMessage response)
        {
            if (request.Method == HttpMethod.Post)
            {
                await LogApplicationAudit("SmsTemplateCreateOrUpdate", request, response);
            }
            else if (request.Method == HttpMethod.Delete)
            {
                await LogApplicationAudit("SmsTemplateDelete", request, response);
            }
        }

        private static async Task LogApplicationAudit(string operationName, HttpRequestMessage request, HttpResponseMessage response)
        {
            //var mandatory = new AuditMandatoryProperties();
            //mandatory.OperationName = operationName;
            //mandatory.ResultType = response.IsSuccessStatusCode ? OperationResult.Success : OperationResult.Failure;
            //mandatory.AddAuditCategory(AuditEventCategory.ResourceManagement);
            //mandatory.AddCallerIdentity(new CallerIdentity(CallerIdentityType.KeyName, SASHelper.GetKeyNameFromToken(request.Headers.Authorization.Parameter)));
            //mandatory.AddTargetResource("Account", request.Headers.GetValues(Constants.AccountHeader)?.FirstOrDefault());

            //var optional = new AuditOptionalProperties();
            //optional.RequestId = request.Headers.GetValues(Constants.OperationTrackingIdHeader)?.FirstOrDefault();
            //optional.ResultDescription = await BuildAuditDescription(request, response);

            //var result = IfxAudit.LogApplicationAudit(mandatory, optional);
            //if (!result)
            //{
            //    request.Headers.TryGetValues(Constants.OperationTrackingIdHeader, out IEnumerable<string> values);
            //    SecurityEventSource.Current.TraceAuditFailure(values?.FirstOrDefault() ?? SecurityEventSource.EmptyTrackingId, request.RequestUri.AbsolutePath);
            //}
            await Task.Delay(0);
        }

        private static async Task<string> BuildAuditDescription(HttpRequestMessage request, HttpResponseMessage response)
        {
            var detail = new
            {
                Path = request.RequestUri.AbsolutePath,
                Request = request.Content != null ? await request.Content.ReadAsStringAsync() : null,
                Response = response.Content != null ? await response.Content.ReadAsStringAsync() : null
            };

            return JsonConvert.SerializeObject(detail);
        }
    }
}
