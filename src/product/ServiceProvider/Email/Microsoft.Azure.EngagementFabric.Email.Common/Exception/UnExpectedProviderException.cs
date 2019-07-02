// <copyright file="UnExpectedProviderException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.Email.Common
{
    public class UnExpectedProviderException : Exception
    {
        public UnExpectedProviderException(string providerName, string message)
            : base($"[{providerName}] {message}")
        {
        }
    }
}
