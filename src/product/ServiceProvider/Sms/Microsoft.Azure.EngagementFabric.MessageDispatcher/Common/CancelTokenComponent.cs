// <copyright file="CancelTokenComponent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System.Threading;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Common
{
    public abstract class CancelTokenComponent : BaseComponent
    {
        private readonly CancellationTokenSource closingCancelTokenSource;

        protected CancelTokenComponent(string component)
            : base(component)
        {
            this.closingCancelTokenSource = new CancellationTokenSource();
        }

        protected CancellationToken CancelToken => this.closingCancelTokenSource.Token;

        protected override void OnClosing()
        {
            base.OnClosing();
            this.closingCancelTokenSource.Cancel();
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            this.closingCancelTokenSource.Dispose();
        }
    }
}
