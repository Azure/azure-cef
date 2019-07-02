// <copyright file="FromHeaderAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Web.Http;
using System.Web.Http.Controllers;

namespace Microsoft.Azure.EngagementFabric.Common.ParameterBind
{
    public class FromHeaderAttribute : ParameterBindingAttribute
    {
        public FromHeaderAttribute()
        {
        }

        public FromHeaderAttribute(string headerName)
        {
            this.HeaderName = headerName;
        }

        public string HeaderName { get; }

        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            return new FromHeaderBinding(parameter, this.HeaderName);
        }
    }
}
