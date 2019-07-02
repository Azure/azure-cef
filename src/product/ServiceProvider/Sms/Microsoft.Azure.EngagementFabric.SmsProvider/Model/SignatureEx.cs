// <copyright file="SignatureEx.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class SignatureEx : Signature
    {
        public SignatureEx()
        {
        }

        public SignatureEx(Signature other)
        {
            this.Value = other.Value;
            this.ChannelType = other.ChannelType;
            this.State = other.State;
            this.Message = other.Message;
            this.EngagementAccount = other.EngagementAccount;
            this.ExtendedCode = other.ExtendedCode;
    }

        [JsonProperty(PropertyName = "ExtendedCode")]
        public new string ExtendedCode { get; set; }
    }
}
