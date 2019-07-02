// -----------------------------------------------------------------------
// <copyright file="SocialExtension.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

namespace Microsoft.Azure.EngagementFabric.SocialProvider.EntityFramework
{
    using System;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Contract;
    using Newtonsoft.Json.Linq;

    public static class SocialExtension
    {
        public static UserInfoRecord ToUserInfoRecord(this UserInfo profile)
        {
            var record = new UserInfoRecord()
            {
                ExpirationTime = null,
                CreatedTime = DateTime.SpecifyKind(profile.CreatedTime, DateTimeKind.Utc),
                ModifiedTime = DateTime.SpecifyKind(profile.ModifiedTime, DateTimeKind.Utc)
            };
            record.Description = new UserInfoRecordDescription(profile.ChannelName);

            using (MemoryStream stream = new MemoryStream(profile.ChannelProperties))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                var desription = (UserInfoRecordDescription)formatter.Deserialize(stream);
                record.Description = new UserInfoRecordDescription(desription);
            }

            return record;
        }
    }
}
