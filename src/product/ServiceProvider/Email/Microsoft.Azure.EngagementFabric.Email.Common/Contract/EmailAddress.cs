// <copyright file="EmailAddress.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Email.Common.Contract
{
    [DataContract]
    public class EmailAddress
    {
        public EmailAddress()
        {
        }

        public EmailAddress(string address, string displayName)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(address, displayName ?? address);

                Address = mailAddress.Address;
                DisplayName = mailAddress.DisplayName;
                User = mailAddress.User;
                Host = mailAddress.Host;
            }
            catch (Exception e)
            {
                throw new ArgumentException(e.Message);
            }
        }

        public EmailAddress(string address)
            : this(address, address)
        {
        }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string User { get; set; }

        [DataMember]
        public string Host { get; set; }
    }
}
