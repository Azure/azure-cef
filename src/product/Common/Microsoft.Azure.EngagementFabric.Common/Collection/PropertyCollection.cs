// -----------------------------------------------------------------------
// <copyright file="PropertyCollection.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.Azure.EngagementFabric.Common.Collection
{
    [Serializable]
    [CollectionDataContract]
    public class PropertyCollection<T> : Dictionary<string, T>
    {
        public PropertyCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public PropertyCollection(int capacity)
            : base(capacity)
        {
        }

        public PropertyCollection(IEqualityComparer<string> comparer)
            : base(comparer)
        {
        }

        public PropertyCollection(IDictionary<string, T> collection)
            : base(collection, StringComparer.OrdinalIgnoreCase)
        {
        }

        // Constructor for deserialization.
        protected PropertyCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public new T this[string name]
        {
            get
            {
                if (ContainsKey(name))
                {
                    return base[name];
                }
                else
                {
                    return default(T);
                }
            }

            set
            {
                if (ContainsKey(name))
                {
                    base[name] = value;
                }
                else
                {
                    Add(name, value);
                }
            }
        }
    }
}
