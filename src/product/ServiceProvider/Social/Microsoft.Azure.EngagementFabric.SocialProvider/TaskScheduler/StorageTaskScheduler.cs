// <copyright file="StorageTaskScheduler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
namespace Microsoft.Azure.EngagementFabric.SocialProvider.Scheduler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Store;
    using Microsoft.Azure.EngagementFabric.SocialProvider.Telemetry;

    public class StorageTaskScheduler
    {
        private readonly TelemetryManager telemetryManager;
        private StoreManager storeManager;

        public StorageTaskScheduler(string connectionString)
        {
            storeManager = new StoreManager(connectionString);
            telemetryManager = new TelemetryManager();
        }

        public void TimerStart()
        {
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000 * 60 * 60;
            timer.Elapsed += new ElapsedEventHandler(CleanStoreAsync);
            timer.Start();
        }

        public async void CleanStoreAsync(object sender, ElapsedEventArgs e)
        {
            // try to delete history data at between 1:00AM and 2:00AM every day
            if (DateTime.UtcNow.Hour == 17)
            {
                SocialProviderEventSource.Current.Info(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.CleanStoreAsync), OperationStates.Starting, string.Empty);
                var storeAgent = await this.storeManager.GetStoreAgent();

                // Delete data in storage 6 month ago
                var dataDelete = this.telemetryManager.DeleteSocialLoginHistoryDataAsync(storeAgent);
                SocialProviderEventSource.Current.Info(SocialProviderEventSource.EmptyTrackingId, this, nameof(this.CleanStoreAsync), OperationStates.Succeeded, string.Empty);
            }
        }
    }
}
