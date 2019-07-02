// <copyright file="OperationStates.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    public class OperationStates
    {
        public const string Starting = nameof(Starting);
        public const string InProgress = nameof(InProgress);
        public const string Committing = nameof(Committing);
        public const string Succeeded = nameof(Succeeded);
        public const string Postponed = nameof(Postponed);
        public const string Skipping = nameof(Skipping);

        public const string Failed = nameof(Failed);
        public const string Faulting = nameof(Faulting);
        public const string FailedNotFaulting = nameof(FailedNotFaulting);
        public const string FailedSwallowingException = nameof(FailedSwallowingException);

        public const string Locking = nameof(Locking);
        public const string Locked = nameof(Locked);
        public const string Unlocked = nameof(Unlocked);

        public const string FoundMatch = nameof(FoundMatch);
        public const string NoMatch = nameof(NoMatch);
        public const string FailedMatch = nameof(FailedMatch);
        public const string Dropped = nameof(Dropped);

        public const string Received = nameof(Received);
        public const string CannotConstruct = nameof(CannotConstruct);

        public const string Set = nameof(Set);

        public const string TimedOut = nameof(TimedOut);
        public const string Empty = "";
    }
}
