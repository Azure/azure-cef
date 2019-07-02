// <copyright file="JsonRemotingRequestBody.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using Microsoft.ServiceFabric.Services.Remoting.V2;

namespace Microsoft.Azure.EngagementFabric.Common.Serialization
{
    internal class JsonRemotingRequestBody : IServiceRemotingRequestMessageBody
    {
        private readonly Dictionary<string, object> parameters = new Dictionary<string, object>();

        public Dictionary<string, object> Parameters => parameters;

        public void SetParameter(int position, string parameName, object parameter)
        {
            this.Parameters[parameName] = parameter;
        }

        public object GetParameter(int position, string parameName, Type paramType)
        {
            return paramType.IsEnum ?
                Enum.Parse(paramType, this.Parameters[parameName].ToString(), true) :
                Convert.ChangeType(this.Parameters[parameName], paramType);
        }
    }
}
