// <copyright file="GlobalParameterAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderDocumentation.Attributes
{
    /// <summary>
    /// Indicates global parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class GlobalParameterAttribute : Attribute
    {
        public GlobalParameterAttribute(string globalParameterName)
        {
            this.GlobalParameterName = globalParameterName;
        }

        public string GlobalParameterName { get; }
    }
}
