// <copyright file="OutputMessageState.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.DispatcherInterface.Contract
{
    [DataContract]
    public enum OutputMessageState
    {
        [EnumMember]
        Unknown,

        [EnumMember]
        Filtered,

        [EnumMember]
        Nonfiltered,

        [EnumMember]
        FilteredFailingDelivery,

        [EnumMember]
        Unfilterable,

        [EnumMember]
        TimeOut
    }
}
