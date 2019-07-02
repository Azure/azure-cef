// <copyright file="MessagePack.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract;
using Microsoft.Azure.EngagementFabric.Sms.Common.Contract;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Model
{
    public class MessagePack
    {
        public InputMessage InputMessage { get; set; }

        public SmsMessageInfoExtension Extension { get; set; }

        public Signature Signature { get; set; }
    }
}
