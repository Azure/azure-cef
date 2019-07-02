// <copyright file="MessageCategory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Sms.Common.Contract
{
    [DataContract]
    public enum MessageCategory
    {
        [DataMember]
        Invalid = 0,

        [DataMember]
        Notification = 1,

        [DataMember]
        Otp = 2,

        [DataMember]
        Promotion = 3,
    }
}
