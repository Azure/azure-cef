// <copyright file="ThreadSafeRandom.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Text;
using System.Threading;

namespace Microsoft.Azure.EngagementFabric.Common.Threading
{
    public class ThreadSafeRandom
    {
        private static readonly Random Global = new Random();

        private ThreadLocal<Random> local;

        public ThreadSafeRandom()
        {
            this.local = new ThreadLocal<Random>(
                () =>
                {
                    int seed;
                    lock (Global)
                    {
                        seed = Global.Next();
                    }

                    return new Random(seed);
                });
        }

        public int Next()
        {
            return this.local.Value.Next();
        }

        public int Next(int min, int max)
        {
            return this.local.Value.Next(min, max);
        }

        public string NextString(int length)
        {
            var data = new byte[length];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)this.local.Value.Next(32, 127);
            }

            return Encoding.ASCII.GetString(data);
        }
    }
}
