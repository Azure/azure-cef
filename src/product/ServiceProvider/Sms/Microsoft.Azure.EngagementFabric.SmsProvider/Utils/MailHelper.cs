// <copyright file="MailHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Sms.Common;
using Microsoft.Azure.EngagementFabric.SmsProvider.Model;
using Microsoft.Exchange.WebServices.Data;
using static Microsoft.Azure.EngagementFabric.SmsProvider.Configuration.ServiceConfiguration;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Utils
{
    public class MailHelper
    {
        public const string TitleFormat = "【模板申请】【{0}】{1}";
        public const string SmsTemplateOpsMailTemplate = @"
            <p><strong>供应商</strong></p>
            <p>$(provider)</p>
            <p><strong>供应商账号</strong></p>
            <p>$(providerId)</p>
            <p><strong>模板类型</strong></p>
            <p>$(category)</p>
            <p><strong>模板名称</strong></p>
            <p>$(name)</p>
            <p><strong>模板内容</strong></p>
            <p>【$(signature)】$(content)</p>
            <p><strong>CEF账号</strong></p>
            <p>$(account)</p>
            <p>&nbsp;</p>";

        private OpsInfo opsInfo;
        private ExchangeService service;

        public MailHelper(OpsInfo opsInfo)
        {
            this.opsInfo = opsInfo;

            this.service = new ExchangeService();
            this.service.UseDefaultCredentials = true;
            this.service.Credentials = new WebCredentials(opsInfo.SenderAddress, opsInfo.SenderPassword);
            this.service.AutodiscoverUrl(opsInfo.SenderAddress, this.RedirectionUrlValidationCallback);
        }

        public void SendMailOnTemplateCreatedOrUpdated(Template template, CredentialAssignment assignment, string trackingId)
        {
            try
            {
                var email = new EmailMessage(this.service);
                email.Subject = string.Format(TitleFormat, template.Signature, template.Name);
                email.Body = new MessageBody(BodyType.HTML, BuildMailBody(SmsTemplateOpsMailTemplate, template, assignment));

                foreach (var receiver in this.opsInfo.ReceiverAddresses)
                {
                    email.ToRecipients.Add(receiver);
                }

                email.SendAndSaveCopy();
            }
            catch (Exception ex)
            {
                SmsProviderEventSource.Current.ErrorException(trackingId, this, nameof(this.SendMailOnTemplateCreatedOrUpdated), OperationStates.Failed, $"Failed to send ops mail for Account={template.EngagementAccount}, TemplateName={template.Name}", ex);
            }
        }

        // The following is a basic redirection validation callback method. It
        // inspects the redirection URL and only allows the Service object to
        // follow the redirection link if the URL is using HTTPS.
        //
        // This redirection URL validation callback provides sufficient security
        // for development and testing of your application. However, it may not
        // provide sufficient security for your deployed application. You should
        // always make sure that the URL validation callback method that you use
        // meets the security requirements of your organization.
        private bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            var result = false;

            var redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials.
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }

            return result;
        }

        private string BuildMailBody(string html, Template template, CredentialAssignment assignment)
        {
            var format = "$({0})";
            var kvs = new Dictionary<string, string>
            {
                { "provider", assignment?.Provider },
                { "providerId", assignment?.ConnectorId },
                { "category", template.Category.ToString() },
                { "name", template.Name },
                { "signature", template.Signature },
                { "content", template.Body },
                { "account", template.EngagementAccount }
            };

            foreach (var kv in kvs)
            {
                var key = string.Format(format, kv.Key);
                html = html.Replace(key, kv.Value);
            }

            return html;
        }
    }
}
