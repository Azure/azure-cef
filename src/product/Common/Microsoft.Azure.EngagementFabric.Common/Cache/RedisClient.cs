// <copyright file="RedisClient.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Microsoft.Azure.EngagementFabric.Common.Cache
{
    public class RedisClient
    {
        private ConnectionMultiplexer connection;
        private int databaseId;
        private ISubscriber subscriber;

        public RedisClient(string connectionString, int databaseId = -1)
        {
            this.connection = ConnectionMultiplexer.Connect(connectionString);
            this.databaseId = databaseId;
            this.subscriber = this.connection.GetSubscriber();
        }

        public async Task<bool> SetAsync(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return false;
            }

            var db = this.connection.GetDatabase(this.databaseId);
            return await db.StringSetAsync(key.ToLower(), value);
        }

        public async Task<bool> SetAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key) || value == null)
            {
                return false;
            }

            var db = this.connection.GetDatabase(this.databaseId);
            var json = JsonConvert.SerializeObject(value);
            return await db.StringSetAsync(key.ToLower(), json);
        }

        public async Task<string> GetAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var db = this.connection.GetDatabase(this.databaseId);
            return await db.StringGetAsync(key.ToLower());
        }

        public async Task<int> GetIntAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var db = this.connection.GetDatabase(this.databaseId);

            int value;
            if (!(await db.StringGetAsync(key.ToLowerInvariant())).TryParse(out value))
            {
                throw new ApplicationException($"{key} does not hold a primitive integer");
            }

            return value;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }

            var db = this.connection.GetDatabase(this.databaseId);
            var json = await db.StringGetAsync(key.ToLower());
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task<bool> DeleteAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            var db = this.connection.GetDatabase(this.databaseId);
            return await db.KeyDeleteAsync(key.ToLower());
        }

        public async Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler)
        {
            await this.subscriber.SubscribeAsync(channel, handler);
        }

        public async Task PublishAsync(RedisChannel channel, RedisValue value)
        {
            await this.subscriber.PublishAsync(channel, value);
        }

        public async Task<int> IncreaseByAsync(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var db = this.connection.GetDatabase(this.databaseId);
            return (int)await db.StringIncrementAsync(key.ToLowerInvariant(), value);
        }

        public async Task<int> DecreaseByAsync(string key, int value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var db = this.connection.GetDatabase(this.databaseId);
            return (int)await db.StringDecrementAsync(key.ToLowerInvariant(), value);
        }

        public async Task<bool> ExistAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var db = this.connection.GetDatabase(this.databaseId);
            return await db.KeyExistsAsync(key.ToLowerInvariant());
        }

        /// <summary>
        /// Try to acquire an application lock with expiry. If and only if there is no given
        /// key in the Redis, it will return true. Before the lock was released or expired,
        /// other caller will got false standing for the lock was occupied
        /// Reminder: it will not reject operation on any key. Application must respect the
        /// returned value and timeout to avoid any conflict. Release a timeout lock may
        /// mis-release the lock occupying by others
        /// </summary>
        /// <param name="key">The key of the lock</param>
        /// <param name="expiry">Expiry of the lock</param>
        /// <returns>True means lock was occupied. Otherwise, false.</returns>
        public async Task<bool> TryAutoLockAsync(string key, TimeSpan? expiry)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var db = this.connection.GetDatabase(this.databaseId);
            return await db.StringSetAsync(
                key.ToLowerInvariant(),
                DateTime.UtcNow.ToString(),
                expiry,
                When.NotExists);
        }

        /// <summary>
        /// Release the application lock
        /// </summary>
        /// <param name="key">The key of the lock</param>
        /// <returns>n/a</returns>
        public async Task ReleaseAutoLockAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var db = this.connection.GetDatabase(this.databaseId);
            await db.KeyDeleteAsync(key.ToLowerInvariant());
        }

        public async Task ExpireAsync(string key, TimeSpan expiry)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            var db = this.connection.GetDatabase(this.databaseId);
            await db.KeyExpireAsync(key.ToLowerInvariant(), expiry);
        }
    }
}
