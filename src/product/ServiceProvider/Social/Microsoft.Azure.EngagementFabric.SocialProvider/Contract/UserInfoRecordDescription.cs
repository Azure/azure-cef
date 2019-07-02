// -----------------------------------------------------------------------
// <copyright file="UserInfoRecordDescription.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using Microsoft.Azure.EngagementFabric.Common;
using Microsoft.Azure.EngagementFabric.Common.Collection;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.Contract
{
    [Serializable]
    [DataContract]
    public class UserInfoRecordDescription : IExtensibleDataObject
    {
        // TODO: ignore binary serialization of ExtensionData until we decide what to put in and whether to store
        [NonSerialized]
        private ExtensionDataObject extensionData;

        public UserInfoRecordDescription()
        {
        }

        public UserInfoRecordDescription(string channel)
        {
            Channel = channel;
            Properties = new PropertyCollection<object>(0);
        }

        public UserInfoRecordDescription(UserInfoRecordDescription description)
        {
            Channel = description.Channel;
            Properties = new PropertyCollection<object>(description.Properties ?? new PropertyCollection<object>(0));
        }

        [DataMember(Name = "ChannelName", Order = 1)]
        public string Channel { get; set; }

        [DataMember(Name = "ChannelProperties", Order = 2)]
        public PropertyCollection<object> Properties { get; set; }

        public ExtensionDataObject ExtensionData
        {
            get
            {
                return extensionData;
            }

            set
            {
                extensionData = value;
            }
        }

        public virtual void Validate()
        {
            Validator.ArgumentNotNullOrEmpty(Channel, nameof(Channel));
            Validator.ArgumentNotNull(Properties, nameof(Properties));
        }
    }
}
