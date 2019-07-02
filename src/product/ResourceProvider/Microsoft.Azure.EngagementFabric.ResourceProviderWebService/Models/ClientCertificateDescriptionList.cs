// <copyright file="ClientCertificateDescriptionList.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    internal class ClientCertificateDescriptionList
    {
        [JsonProperty("clientCertificates")]
        public IEnumerable<ClientCertificateDescription> ClientCertificates { get; set; }
    }
}
