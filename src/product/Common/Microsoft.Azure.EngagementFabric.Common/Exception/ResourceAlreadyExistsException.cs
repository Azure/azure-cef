// <copyright file="ResourceAlreadyExistsException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Common
{
    [Serializable]
    public class ResourceAlreadyExistsException : Exception
    {
        public ResourceAlreadyExistsException(string message)
            : base(message)
        {
        }

        protected ResourceAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
