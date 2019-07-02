// -----------------------------------------------------------------------
// <copyright file="EventSourceHelper.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.IO;

namespace Microsoft.Azure.EngagementFabric.Common.Telemetry
{
    public static class EventSourceHelper
    {
        public const byte EventVersion = 1;
        public const string EventSourceMessageFormat = "[{0}] {1} {2} {3} {4} {5} {7}(@{6})";

        public const int MaxStringLength = 30000;

        public static string TruncateIfTooLarge(string targetString, ref int remainingLength)
        {
            if (remainingLength <= 0)
            {
                return string.Empty;
            }

            remainingLength = Math.Min(EventSourceHelper.MaxStringLength, remainingLength);

            targetString = targetString ?? string.Empty;
            if (targetString.Length > remainingLength)
            {
                targetString = targetString.Substring(0, remainingLength);
                remainingLength = 0;
            }
            else
            {
                remainingLength -= targetString.Length;
            }

            return targetString;
        }

        public static string GetFileName(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return string.Empty;
            }

            return Path.GetFileName(filePath);
        }

        public static CallerDetails GetCallerDetails(object source)
        {
            if (source == null)
            {
                return CallerDetails.Empty;
            }

            string callerId;
            string callerState = string.Empty;
            if ((callerId = source as string) == null)
            {
                Type sourceAsType;
                Exception otherException;
                if ((sourceAsType = source as Type) != null)
                {
                    callerId = sourceAsType.Name;
                }
                else if ((otherException = source as Exception) != null)
                {
                    callerId = otherException.GetType().Name;
                }
                else
                {
                    callerId = source.ToString();
                }
            }

            ITraceStateProvider traceStateProvider;
            if ((traceStateProvider = source as ITraceStateProvider) != null)
            {
                callerState = traceStateProvider.GetTraceState();
            }

            return new CallerDetails(callerId, callerState);
        }

        public static string FormatMessageAndException(string message, Exception exception)
        {
            return string.IsNullOrWhiteSpace(message) ? exception.ToString() : $"{message}\n{exception}";
        }
    }
}
