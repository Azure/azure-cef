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

    public partial class Signature
    {
        /// <summary>
        /// Initializes a new instance of the Signature class.
        /// </summary>
        public Signature()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the Signature class.
        /// </summary>
        /// <param name="type">Possible values include: 'Industry',
        /// 'Marketing', 'Both'</param>
        /// <param name="state">Possible values include: 'Active', 'Pending',
        /// 'Forbidden', 'Disabled'</param>
        public Signature(string signatureProperty, string type, string state, string stateMessage = default(string), string extendedCode = default(string))
        {
            SignatureProperty = signatureProperty;
            Type = type;
            State = state;
            StateMessage = stateMessage;
            ExtendedCode = extendedCode;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "signature")]
        public string SignatureProperty { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Industry', 'Marketing',
        /// 'Both'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Active', 'Pending',
        /// 'Forbidden', 'Disabled'
        /// </summary>
        [JsonProperty(PropertyName = "state")]
        public string State { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "stateMessage")]
        public string StateMessage { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "extendedCode")]
        public string ExtendedCode { get; set; }

        /// <summary>
        /// Validate the object.
        /// </summary>
        /// <exception cref="ValidationException">
        /// Thrown if validation fails
        /// </exception>
        public virtual void Validate()
        {
            if (SignatureProperty == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "SignatureProperty");
            }
            if (Type == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "Type");
            }
            if (State == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "State");
            }
        }
    }
}
