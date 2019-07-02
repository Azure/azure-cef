// <copyright file="ServiceConfigureSetting.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Configuration
{
    // TODO: read from configuration
    public class ServiceConfigureSetting
    {
        public TimeSpan EventTimeToLive
        {
            get
            {
                return TimeSpan.FromMinutes(5);
            }
        }
    }
}
