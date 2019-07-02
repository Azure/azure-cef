// <copyright file="StoreTaskScheduler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>
namespace Microsoft.Azure.EngagementFabric.OtpProvider.Scheduler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;
    using Microsoft.Azure.EngagementFabric.Common.Telemetry;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Store;
    using Microsoft.Azure.EngagementFabric.OtpProvider.Telemetry;
    using Microsoft.WindowsAzure.Storage;

    public class StoreTaskScheduler
    {
        private readonly TelemetryManager telemetryManager;

        public StoreTaskScheduler(IOtpStoreFactory factory, string storageConnectionString)
        {
            this.telemetryManager = new TelemetryManager(storageConnectionString);
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
            // try to delete old data at Beijing time 1:00AM ~ 2:00AM every day
            if (DateTime.UtcNow.Hour == 17)
            {
                OtpProviderEventSource.Current.Info(OtpProviderEventSource.EmptyTrackingId, this, nameof(this.CleanStoreAsync), OperationStates.Starting, string.Empty);

                // Delete data in storage 6 month ago
                var dataDelete = this.telemetryManager.DeleteOtpCodeHistoryDataAsync();
                await Task.CompletedTask;
            }
        }
    }
}
