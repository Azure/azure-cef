// <copyright file="SASInvalidException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.Common
{
    public class SASInvalidException : Exception
    {
        public SASInvalidException(string message)
            : base(message)
        {
        }
    }
}
