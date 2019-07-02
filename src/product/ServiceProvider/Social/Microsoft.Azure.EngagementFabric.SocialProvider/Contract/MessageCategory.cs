// <copyright file="MessageCategory.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Contract
{
    [DataContract]
    public enum MessageCategory
    {
        [EnumMember]
        Invalid = 0,

        [EnumMember]
        WeChat = 1,

        [EnumMember]
        Weibo = 2,

        [EnumMember]
        QQ = 3,
    }
}
