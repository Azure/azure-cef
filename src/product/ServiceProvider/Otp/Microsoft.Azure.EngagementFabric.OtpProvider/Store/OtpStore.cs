// <copyright file="OtpStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

namespace Microsoft.Azure.EngagementFabric.OtpProvider.Store
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading.Tasks;
    // using System.Web.Script.Serialization;
    using Microsoft.Azure.EngagementFabric.Common;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Contract;
    using Microsoft.Azure.EngagementFabric.OtpProvider.EntityFramework;
    using Microsoft.Azure.EngagementFabric.ProviderInterface.Contract;
    using Microsoft.WindowsAzure.Storage;

    public class OtpStore : IOtpStore
    {
        private readonly string connectionString;

        public OtpStore(string connectionString, int maxPoolSize)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
            {
                MaxPoolSize = maxPoolSize
            };
            connectionStringBuilder.ConnectRetryCount = 3;
            connectionStringBuilder.ConnectRetryInterval = 10;

            var entityStringBuilder = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                Metadata = "res://*/EntityFramework.OtpServiceDataModel.csdl|res://*/EntityFramework.OtpServiceDataModel.ssdl|res://*/EntityFramework.OtpServiceDataModel.msl",
                ProviderConnectionString = connectionStringBuilder.ToString()
            };

            this.connectionString = entityStringBuilder.ConnectionString;
        }

        public async Task<OtpCode> CreateorUpdateOtpCodeAsync(string engagementAccount, string phoneNumber, string code, int expireTime)
        {
            using (var ctx = new OtpEntities(this.connectionString))
            {
                var otpCode = await ctx.OtpCodes.Where(f => f.EngagementAccount == engagementAccount && f.PhoneNumber == phoneNumber).SingleOrDefaultAsync();

                if (otpCode == null)
                {
                    otpCode = new OtpCode
                    {
                        PhoneNumber = phoneNumber,
                        EngagementAccount = engagementAccount,
                        Code = code,
                        CreatedTime = DateTime.UtcNow,
                        ExpiredTime = DateTime.UtcNow.AddSeconds(expireTime)
                    };
                    ctx.OtpCodes.Add(otpCode);
                }
                else
                {
                    otpCode.Code = code;
                    otpCode.CreatedTime = DateTime.UtcNow;
                    otpCode.ExpiredTime = DateTime.UtcNow.AddSeconds(expireTime);
                }

                await ctx.SaveChangesAsync();
                return otpCode;
            }
        }

        public async Task DeleteOtpCodeAsync(string engagementAccount, string phoneNumber)
        {
            using (var ctx = new OtpEntities(this.connectionString))
            {
                {
                    var entity = await ctx.OtpCodes.Where(f => f.EngagementAccount == engagementAccount && f.PhoneNumber == phoneNumber).SingleOrDefaultAsync();

                    if (entity == null)
                    {
                        return;
                    }

                    ctx.OtpCodes.Remove(entity);
                    await ctx.SaveChangesAsync();
                }
            }
        }

        public async Task DeleteOtpAccountDataAsync(string account)
        {
            using (var ctx = new OtpEntities(this.connectionString))
            {
                var entities = await ctx.OtpCodes.Where(f => f.EngagementAccount == account).ToListAsync();
                if (entities == null || !entities.Any())
                {
                    return;
                }

                ctx.OtpCodes.RemoveRange(entities);
                await ctx.SaveChangesAsync();
            }
        }

        public async Task<OtpCode> QueryOtpCodeAsync(string engagementAccount, string phoneNumber)
        {
            using (var ctx = new OtpEntities(this.connectionString))
            {
                return await ctx.OtpCodes.Where(f => f.EngagementAccount == engagementAccount && f.PhoneNumber == phoneNumber).SingleOrDefaultAsync();
            }
        }

        public async Task DeleteOtpCodeByTimeAsync(DateTime expiredTime)
        {
            using (var ctx = new OtpEntities(this.connectionString))
            {
                {
                    // Delete records created 1 day ago
                    var entities = await ctx.OtpCodes.Where(f => f.ExpiredTime < expiredTime).ToListAsync();

                    if (entities == null || !entities.Any())
                    {
                        return;
                    }

                    ctx.OtpCodes.RemoveRange(entities);
                    await ctx.SaveChangesAsync();
                }
            }
        }
    }
}
