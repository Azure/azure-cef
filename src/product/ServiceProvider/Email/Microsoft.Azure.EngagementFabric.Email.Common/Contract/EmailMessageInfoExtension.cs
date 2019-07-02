// <copyright file="EmailMessageInfoExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    public class EmailMessageInfoExtension
    {
        public EmailAccount EmailAccount { get; set; }

        public SenderAddress SenderAddress { get; set; }

        public string DisplayName { get; set; }

        public string Title { get; set; }

        public TargetType TargetType { get; set; }

        public bool? EnableUnSubscribe { get; set; }

        public static EmailMessageInfoExtension Parse(string extension)
        {
            if (!string.IsNullOrEmpty(extension))
            {
                return JsonConvert.DeserializeObject<EmailMessageInfoExtension>(extension);
            }

            return new EmailMessageInfoExtension();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
