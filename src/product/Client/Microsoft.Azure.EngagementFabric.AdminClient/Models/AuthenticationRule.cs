// <auto-generated>
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// Licensed under the MIT License. See License.txt in the project root for
// license information.
//
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Microsoft.Azure.EngagementFabric.AdminClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class AuthenticationRule
    {
        /// <summary>
        /// Initializes a new instance of the AuthenticationRule class.
        /// </summary>
        public AuthenticationRule()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the AuthenticationRule class.
        /// </summary>
        public AuthenticationRule(string keyName = default(string), string primaryKey = default(string), string secondaryKey = default(string))
        {
            KeyName = keyName;
            PrimaryKey = primaryKey;
            SecondaryKey = secondaryKey;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "keyName")]
        public string KeyName { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "primaryKey")]
        public string PrimaryKey { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "secondaryKey")]
        public string SecondaryKey { get; set; }

    }
}
