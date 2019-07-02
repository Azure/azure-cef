// <copyright file="QuotaExceededException.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Common
{
    [Serializable]
    public class QuotaExceededException : Exception
    {
        public QuotaExceededException(string account, string quotaName, long remaining)
            : base($"No enough quota ({quotaName} of {account}) for this request. Remaining quota is {remaining}")
        {
            this.Account = account;
            this.QuotaName = quotaName;
            this.Remaining = remaining;
        }

        protected QuotaExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.Account = info.GetString(nameof(this.Account));
            this.QuotaName = info.GetString(nameof(this.QuotaName));
            this.Remaining = info.GetInt32(nameof(this.Remaining));
        }

        public string Account { get; }

        public string QuotaName { get; }

        public long Remaining { get; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(nameof(this.Account), this.Account);
            info.AddValue(nameof(this.QuotaName), this.QuotaName);
            info.AddValue(nameof(this.Remaining), this.Remaining);

            base.GetObjectData(info, context);
        }
    }
}
