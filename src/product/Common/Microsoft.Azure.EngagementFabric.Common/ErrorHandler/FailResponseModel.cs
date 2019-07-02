// -----------------------------------------------------------------------
// <copyright file="FailResponseModel.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Common
{
    public class FailResponseModel
    {
        public FailResponseModel()
        {
        }

        public FailResponseModel(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
