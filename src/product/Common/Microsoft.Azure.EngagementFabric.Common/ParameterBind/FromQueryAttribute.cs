// <copyright file="FromQueryAttribute.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Web.Http;
using System.Web.Http.Controllers;

namespace Microsoft.Azure.EngagementFabric.Common.ParameterBind
{
    public class FromQueryAttribute : ParameterBindingAttribute
    {
        public FromQueryAttribute()
        {
        }

        public FromQueryAttribute(string queryName)
        {
            this.QueryName = queryName;
        }

        public string QueryName { get; }

        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            return new FromQueryBinding(parameter, this.QueryName);
        }
    }
}
