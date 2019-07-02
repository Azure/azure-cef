// <copyright file="BaseComponent.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.EngagementFabric.Common.Telemetry;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Common
{
    public abstract class BaseComponent : IComponent, ITraceStateProvider
    {
        protected BaseComponent(string component)
        {
            this.Component = component;
            this.Lock = new object();
        }

        public event EventHandler Opening;

        public event EventHandler Opened;

        public event EventHandler Closing;

        public event EventHandler Closed;

        public event EventHandler<FirstChanceExceptionEventArgs> Faulted;

        protected string Component { get; }

        protected object Lock { get; }

        public virtual string GetTraceState()
        {
            return $"Component={this.Component}";
        }

        public async Task OpenAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.OnOpening();

                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.OpenAsync), OperationStates.Starting, string.Empty);
                await this.OnOpenAsync(cancellationToken);
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.OpenAsync), OperationStates.Succeeded, string.Empty);

                this.OnOpened();
            }
            catch (Exception exception)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.OpenAsync), OperationStates.Faulting, string.Empty, exception);
                this.Fault(exception);
                throw;
            }
        }

        public async Task CloseAsync(CancellationToken cancellationToken)
        {
            try
            {
                this.OnClosing();

                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CloseAsync), OperationStates.Starting, string.Empty);
                await this.OnCloseAsync(cancellationToken);
                MessageDispatcherEventSource.Current.Info(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CloseAsync), OperationStates.Succeeded, string.Empty);

                this.OnClosed();
            }
            catch (Exception exception)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.CloseAsync), OperationStates.Failed, string.Empty, exception);
                throw;
            }
        }

        protected void Fault(Exception exception)
        {
            try
            {
                MessageDispatcherEventSource.Current.Warning(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.Fault), OperationStates.Starting, exception?.ToString());
                this.OnFaulted(exception);
                MessageDispatcherEventSource.Current.Warning(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.Fault), OperationStates.Succeeded, string.Empty);
            }
            catch (Exception ex)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.Fault), OperationStates.Failed, string.Empty, ex);
                throw;
            }
        }

        protected abstract Task OnOpenAsync(CancellationToken cancellationToken);

        protected abstract Task OnCloseAsync(CancellationToken cancellationToken);

        protected virtual void OnOpening()
        {
            try
            {
                this.Opening?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception callbackException)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.OnOpening), OperationStates.Failed, string.Empty, callbackException);
                throw;
            }
        }

        protected virtual void OnOpened()
        {
            try
            {
                this.Opened?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception callbackException)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.OnOpened), OperationStates.Failed, string.Empty, callbackException);
                throw;
            }
        }

        protected virtual void OnClosing()
        {
            try
            {
                this.Closing?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception callbackException)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.OnClosing), OperationStates.Failed, string.Empty, callbackException);
                throw;
            }
        }

        protected virtual void OnClosed()
        {
            try
            {
                this.Closed?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception callbackException)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.OnClosed), OperationStates.Failed, string.Empty, callbackException);
                throw;
            }
        }

        protected virtual void OnFaulted(Exception exception)
        {
            try
            {
                this.Faulted?.Invoke(this, new FirstChanceExceptionEventArgs(exception));
            }
            catch (Exception callbackException)
            {
                MessageDispatcherEventSource.Current.ErrorException(MessageDispatcherEventSource.EmptyTrackingId, this, nameof(this.OnFaulted), OperationStates.Failed, string.Empty, callbackException);
                throw;
            }
        }
    }
}
