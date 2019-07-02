// <copyright file="FromHeaderBinding.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace Microsoft.Azure.EngagementFabric.Common.ParameterBind
{
    public class FromHeaderBinding : HttpParameterBinding
    {
        private readonly string headerName;

        public FromHeaderBinding(HttpParameterDescriptor descriptor, string headerName)
            : base(descriptor)
        {
            this.headerName = headerName ?? descriptor.ParameterName;
        }

        public override async Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            IEnumerable<string> values;
            if (actionContext.Request.Headers.TryGetValues(this.headerName, out values))
            {
                try
                {
                    actionContext.ActionArguments[this.Descriptor.ParameterName] = Convert.ChangeType(values.FirstOrDefault(), this.Descriptor.ParameterType);
                }
                catch
                {
                    throw new HttpRequestInvalidParameterTypeException(this.Descriptor.ParameterName, this.Descriptor.ParameterType);
                }
            }
            else if (this.Descriptor.IsOptional)
            {
                actionContext.ActionArguments[this.Descriptor.ParameterName] = this.Descriptor.DefaultValue;
            }
            else
            {
                throw new HttpRequestMissingParameterException(this.Descriptor.ParameterName);
            }

            await Task.CompletedTask;
        }
    }
}
