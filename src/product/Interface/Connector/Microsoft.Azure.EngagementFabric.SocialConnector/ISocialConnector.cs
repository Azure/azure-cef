// <copyright file="ISocialConnector.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.SocialConnector
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.EngagementFabric.Common.Collection;

    public interface ISocialConnector
    {
        /// <summary>
        /// Retrieve profile information from backend channel
        /// </summary>
        /// <param name="request">accesstoken,openid....</param>\
        /// <returns>
        /// 1. Social profile
        /// </returns>
        Task<PropertyCollection<string>> GetSocialProfileAsync(PropertyCollection<object> request);
    }
}
