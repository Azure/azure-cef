// <copyright file="ClientCertificateDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.ResourceProviderWebService.Models
{
    internal class ClientCertificateDescription
    {
        [JsonProperty("thumbprint")]
        public string Thumbprint { get; set; }

        [JsonProperty("notBefore")]
        public DateTime NotBefore { get; set; }

        [JsonProperty("notAfter")]
        public DateTime NotAfter { get; set; }

        [JsonProperty("certificate")]
        public string Certificate { get; set; }
    }
}
