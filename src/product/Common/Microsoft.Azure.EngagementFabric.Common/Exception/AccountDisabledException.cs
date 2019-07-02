// <copyright file="AccountDisabledException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.Common
{
    public class AccountDisabledException : Exception
    {
        public AccountDisabledException(string message)
            : base(message)
        {
        }
    }
}
