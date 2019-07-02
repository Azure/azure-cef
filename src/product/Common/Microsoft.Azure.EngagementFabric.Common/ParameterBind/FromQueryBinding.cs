// <copyright file="FromQueryBinding.cs" company="Microsoft Corporation">
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
    public class FromQueryBinding : HttpParameterBinding
    {
        private readonly string queryName;

        public FromQueryBinding(HttpParameterDescriptor descriptor, string queryName)
            : base(descriptor)
        {
            this.queryName = queryName ?? descriptor.ParameterName;
        }

        public override async Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(actionContext.Request.RequestUri.Query);
            var value = queryDictionary.Get(queryName);
            if (value != null)
            {
                try
                {
                    actionContext.ActionArguments[this.Descriptor.ParameterName] = Convert.ChangeType(value, this.Descriptor.ParameterType);
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
