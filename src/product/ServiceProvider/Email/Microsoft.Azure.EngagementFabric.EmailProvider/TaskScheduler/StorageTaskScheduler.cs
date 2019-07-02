// <copyright file="StorageTaskScheduler.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Timers;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;
using Microsoft.Azure.EngagementFabric.Email.Common;
using Microsoft.Azure.EngagementFabric.EmailProvider.Report;
using Microsoft.Azure.EngagementFabric.EmailProvider.Utils;

namespace Microsoft.Azure.EngagementFabric.EmailProvider.Scheduler
{
    public class StorageTaskScheduler
    {
        private IReportManager reportManager;

        public StorageTaskScheduler(IReportManager reportManage)
        {
            this.reportManager = reportManage;
        }

        public void TimerStart()
        {
            var timer = new Timer();

            // run every 1 hour
            timer.Interval = 1000 * 60 * Constants.ReportPullingIntervalByMinutes;
            timer.Elapsed += new ElapsedEventHandler(PullReportAsync);
            timer.Start();
        }

        public void PullReportAsync(object sender, ElapsedEventArgs e)
        {
            try
            {
                EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportAsync), OperationStates.Starting, string.Empty);
                this.reportManager.PullReportsAsync().Wait();
                EmailProviderEventSource.Current.Info(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportAsync), OperationStates.Succeeded, string.Empty);
            }
            catch (Exception ex)
            {
                EmailProviderEventSource.Current.ErrorException(EmailProviderEventSource.EmptyTrackingId, this, nameof(this.PullReportAsync), OperationStates.Failed, string.Empty, ex);
            }
        }
    }
}
