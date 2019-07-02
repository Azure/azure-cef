// -----------------------------------------------------------------------
// <copyright file="ICodePackageActivationContextExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Fabric;
using System.Linq;

namespace Microsoft.Azure.EngagementFabric.Common.Extension
{
    public static class ICodePackageActivationContextExtension
    {
        public static T GetConfig<T>(this ICodePackageActivationContext context, string sectionName, string parameterName)
        {
            // Get config package
            var configurationPackage = context?.GetConfigurationPackageObject("Config");
            if (configurationPackage == null)
            {
                throw new InvalidOperationException("ConfigurationPackage is not found.");
            }

            // Get config section
            var configurationSection = configurationPackage.Settings.Sections.FirstOrDefault(s => s.Name.Equals(sectionName, StringComparison.OrdinalIgnoreCase));
            if (configurationSection == null)
            {
                throw new InvalidOperationException($"ConfigurationSection '{sectionName}' is not found.");
            }

            // Get config value
            var configurationProperty = configurationSection.Parameters[parameterName];
            if (configurationProperty == null)
            {
                throw new ArgumentNullException($"ConfigurationProperty '{parameterName}' is null.");
            }

            return (T)Convert.ChangeType(configurationProperty.Value, typeof(T));
        }
    }
}
