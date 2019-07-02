// -----------------------------------------------------------------------
// <copyright file="UserInfoResult.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.SocialProvider.Contract;

namespace Microsoft.Azure.EngagementFabric.SocialProvider.EntityFramework
{
    public class UserInfoResult
    {
        public ActionType Action { get; set; }

        public UserInfo UserInfo { get; set; }
    }
}