// <copyright file="JsonRemotingResponseBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace Microsoft.Azure.EngagementFabric.Common.Serialization
{
    internal class JsonRemotingResponseBody : IServiceRemotingResponseMessageBody
    {
        private object value;

        public object Value
        {
            get
            {
                return value;
            }

            set
            {
                this.value = value;
            }
        }

        public void Set(object response)
        {
            this.Value = response;
        }

        public object Get(Type paramType)
        {
            return this.Value;
        }
    }
}
