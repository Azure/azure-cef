// <copyright file="ExceptionExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Fabric;

namespace Microsoft.Azure.EngagementFabric.Common.Extension
{
    public static class ExceptionExtension
    {
        public static bool IsFabricCriticalException(this Exception ex)
        {
            int iterations = 0;
            while (ex != null && iterations < 10)
            {
                if (ex is FabricNotPrimaryException || ex is FabricNotReadableException || ex is FabricObjectClosedException)
                {
                    return true;
                }

                ex = ex.InnerException;
                iterations++;
            }

            return false;
        }
    }
}
