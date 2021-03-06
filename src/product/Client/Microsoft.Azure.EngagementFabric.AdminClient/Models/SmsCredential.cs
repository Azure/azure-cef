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
    using Microsoft.Rest;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class SmsCredential
    {
        /// <summary>
        /// Initializes a new instance of the SmsCredential class.
        /// </summary>
        public SmsCredential()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the SmsCredential class.
        /// </summary>
        /// <param name="channelType">Possible values include: 'Industry',
        /// 'Marketing', 'Both'</param>
        public SmsCredential(string provider, string id, string channelType, object properties)
        {
            Provider = provider;
            Id = id;
            ChannelType = channelType;
            Properties = properties;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "provider")]
        public string Provider { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Industry', 'Marketing',
        /// 'Both'
        /// </summary>
        [JsonProperty(PropertyName = "channelType")]
        public string ChannelType { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "properties")]
        public object Properties { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (Provider == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Provider");
            }
            if (Id == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Id");
            }
            if (ChannelType == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "ChannelType");
            }
            if (Properties == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Properties");
            }
        }
    }
}
