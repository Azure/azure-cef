// <copyright file="SmsMessageInfoExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    public class SmsMessageInfoExtension
    {
        public ChannelType ChannelType { get; set; }

        public MessageCategory MessageCategory { get; set; }

        // Extended code will contains three parts: company(optional), signature, custom (optional)
        public List<string> ExtendedCodes { get; set; }

        public static SmsMessageInfoExtension Parse(string extension)
        {
            if (!string.IsNullOrEmpty(extension))
            {
                return JsonConvert.DeserializeObject<SmsMessageInfoExtension>(extension);
            }

            return new SmsMessageInfoExtension();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
