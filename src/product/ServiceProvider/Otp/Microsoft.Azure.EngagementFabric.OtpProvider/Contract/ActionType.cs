// -----------------------------------------------------------------------
// <copyright file="ActionType.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Contract
{
    public enum ActionType
    {
        Start,
        CheckDelete,
        ExpireDelete
    }
}