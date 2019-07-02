// -----------------------------------------------------------------------
// <copyright file="Validator.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.Common
{
    public static class Validator
    {
        private static readonly Dictionary<Type, Func<string, string, Exception>> KnownExceptionFactories = new Dictionary<Type, Func<string, string, Exception>>
        {
            { typeof(ApplicationException), (msg, param) => new ApplicationException(msg) },
            { typeof(ArgumentException), (msg, param) => new ArgumentException(msg) },
            { typeof(ArgumentOutOfRangeException), (msg, param) => new ArgumentOutOfRangeException(param, msg) },
            { typeof(ArgumentNullException), (msg, param) => new ArgumentNullException(param, msg) },
            { typeof(InvalidOperationException), (msg, param) => new InvalidOperationException(msg) },
            { typeof(IOException), (msg, param) => new IOException(msg) },
            { typeof(JsonException), (msg, param) => new JsonException(msg) },
            { typeof(JsonSerializationException), (msg, param) => new JsonSerializationException(msg) },
            { typeof(NotImplementedException), (msg, param) => new NotImplementedException(msg) },
            { typeof(NotSupportedException), (msg, param) => new NotSupportedException(msg) },
            { typeof(ObjectDisposedException), (msg, param) => new ObjectDisposedException(param, msg) },
            { typeof(OperationCanceledException), (msg, param) => new OperationCanceledException(msg) },
            { typeof(UnauthorizedAccessException), (msg, param) => new UnauthorizedAccessException(msg) },
            { typeof(ObjectNotFoundException), (msg, param) => new ObjectNotFoundException(msg) },
            { typeof(InvalidDataContractException), (msg, param) => new InvalidDataContractException(msg) },

            // CEF Common Exception
            { typeof(ResourceNotFoundException), (msg, param) => new ResourceNotFoundException(msg) },
            { typeof(ChannelInvalidException), (msg, param) => new ChannelInvalidException(msg) }
        };

        public static void ArgumentNotNull(object value, string paramName)
        {
            if (value == null)
            {
                var message = $"The argument {paramName} is null.";
                throw new ArgumentNullException(paramName, message);
            }
        }

        public static void StringEquals(string a, string b, StringComparison compare, string paramName)
        {
            if (!string.Equals(a, b, compare))
            {
                var message = $"The argument '{a}' was not equal to '{b}'.";
                throw new ArgumentException(message, paramName);
            }
        }

        public static void ArgumentIsNull(object value, string paramName)
        {
            if (value != null)
            {
                var message = $"The argument {paramName} is not null.";
                throw new ArgumentException(message, paramName);
            }
        }

        public static void ArgumentNotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
            {
                var message = $"The argument {paramName} is null or empty.";
                if (value == null)
                {
                    throw new ArgumentNullException(paramName, message);
                }

                throw new ArgumentException(message, paramName);
            }
        }

        public static void ArgumentNotNullOrrWhiteSpace(string value, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                var message = $"The argument {paramName} is null or whitespace.";
                if (value == null)
                {
                    throw new ArgumentNullException(paramName, message);
                }

                throw new ArgumentException(message, paramName);
            }
        }

        public static Guid ArgumentValidGuid(string value, string paramName)
        {
            Guid guid = Guid.Empty;
            if (!Guid.TryParse(value, out guid))
            {
                var message = $"The argument {paramName} is not a valid Guid.";
                throw new ArgumentException(message, paramName);
            }

            if (Guid.Empty.Equals(guid))
            {
                var message = $"The argument {paramName} is an empty Guid.";
                throw new ArgumentException(message, paramName);
            }

            return guid;
        }

        public static Uri ArgumentValidUri(string value, string paramName)
        {
            Uri uri;
            if (!Uri.TryCreate(value, UriKind.Absolute, out uri))
            {
                var message = $"The argument {paramName} is not a valid Uri.";
                throw new ArgumentException(message, paramName);
            }

            return uri;
        }

        public static void ArgumentInRange<TComparable>(TComparable value, TComparable minInclusive, TComparable maxInclusive, string paramName)
            where TComparable : IComparable<TComparable>
        {
            if (value.CompareTo(minInclusive) < 0 || value.CompareTo(maxInclusive) > 0)
            {
                string message = $"The argument {paramName} was out of range. Allowed values are from {1} to {2}.";
                throw new ArgumentOutOfRangeException(paramName, value, message);
            }
        }

        public static void IsTrue<TException>(bool condition, string paramName, string formatString, params object[] args)
            where TException : Exception
        {
            if (!condition)
            {
                var message = formatString;
                if (args != null && args.Length != 0)
                {
                    message = string.Format(formatString, args);
                }

                throw CreateException<TException>(message, paramName);
            }
        }

        private static Exception CreateException<TException>(string message, string paramName)
            where TException : Exception
        {
            Func<string, string, Exception> factory;
            if (KnownExceptionFactories.TryGetValue(typeof(TException), out factory))
            {
                return factory(message, paramName);
            }

            return new InvalidOperationException(message);
        }
    }
}
