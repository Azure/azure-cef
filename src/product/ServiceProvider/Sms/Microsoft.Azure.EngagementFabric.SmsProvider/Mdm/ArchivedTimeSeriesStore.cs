// <copyright file="ArchivedTimeSeriesStore.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace Microsoft.Azure.EngagementFabric.SmsProvider.Mdm
{
    internal class ArchivedTimeSeriesStore : IArchivedTimeSeriesStore
    {
        private readonly CloudBlobContainer container;
        private readonly string blobNamePrefix;
        private readonly TimeSpan leaseTTL = TimeSpan.FromSeconds(60);
        private string leaseId;

        public ArchivedTimeSeriesStore(
            string connectionString,
            string containerName,
            string blobNamePrefix)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var client = storageAccount.CreateCloudBlobClient();

            this.container = client.GetContainerReference(containerName.ToLowerInvariant());
            this.container.CreateIfNotExists();

            this.blobNamePrefix = blobNamePrefix;
        }

        public async Task PutTimeSeries(string name, TimeSeries series)
        {
            var blob = this.GetBlobReference(name) as CloudBlockBlob;
            await blob.UploadTextAsync(JsonConvert.SerializeObject(series));
        }

        public async Task<TimeSeries> GetTimeSeries(string name)
        {
            var blob = this.GetBlobReference(name);
            if (!await blob.ExistsAsync())
            {
                return null;
            }

            var content = await blob.DownloadTextAsync();
            return JsonConvert.DeserializeObject<TimeSeries>(content);
        }

        public async Task<bool> IsTimeSeriesExist(string name)
        {
            var blob = this.GetBlobReference(name);
            return await blob.ExistsAsync();
        }

        public async Task<bool> AcquireLeaseAsync()
        {
            try
            {
                this.leaseId = await this.container.AcquireLeaseAsync(this.leaseTTL);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task RenewLeaseAsync()
        {
            await this.container.RenewLeaseAsync(new AccessCondition
            {
                LeaseId = this.leaseId
            });
        }

        public async Task ReleaseLeaseAsync()
        {
            await this.container.ReleaseLeaseAsync(new AccessCondition
            {
                LeaseId = this.leaseId
            });
        }

        private CloudBlockBlob GetBlobReference(string name)
        {
            var blobName = string.IsNullOrWhiteSpace(this.blobNamePrefix)
                ? $"{name}.json"
                : $"{this.blobNamePrefix}/{name}.json";

            return this.container.GetBlockBlobReference(blobName.ToLowerInvariant());
        }
    }
}
