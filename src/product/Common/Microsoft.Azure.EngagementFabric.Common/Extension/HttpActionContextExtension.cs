// <copyright file="HttpActionContextExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Web.Http.Controllers;

namespace Microsoft.Azure.EngagementFabric.Common.Extension
{
    public static class HttpActionContextExtension
    {
        public static T GetControllerConfiguration<T>(this HttpActionContext context, string key)
        {
            object obj;
            if (context.ControllerContext.Configuration.Properties.TryGetValue(key, out obj))
            {
                return (T)obj;
            }
            else
            {
                return default(T);
            }
        }
    }
}
