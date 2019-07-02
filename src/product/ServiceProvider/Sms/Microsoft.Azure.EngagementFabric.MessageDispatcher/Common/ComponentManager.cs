// <copyright file="ComponentManager.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation.// Licensed under the MIT license.
// </copyright>

using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.EngagementFabric.MessageDispatcher.Common
{
    public sealed class ComponentManager : BaseComponent
    {
        private readonly List<IComponent> components;
        private readonly string traceName;
        private readonly EventHandler onInnerComponentClosed;
        private readonly EventHandler<FirstChanceExceptionEventArgs> onInnerComponentFaulted;

        public ComponentManager(string traceName, string component)
            : base(component)
        {
            this.traceName = traceName;
            this.components = new List<IComponent>();
            this.onInnerComponentClosed = this.OnInnerComponentClosed;
            this.onInnerComponentFaulted = this.OnInnerComponentFaulted;
        }

        public void Add(IComponent component)
        {
            lock (this.Lock)
            {
                component.Closed += this.onInnerComponentClosed;
                component.Faulted += this.onInnerComponentFaulted;
                this.components.Add(component);
            }
        }

        protected override async Task OnOpenAsync(CancellationToken cancellationToken)
        {
            IComponent[] componentsToOpen;
            lock (this.Lock)
            {
                // Copy the collection into an array in case any new component added while opening
                componentsToOpen = this.components.ToArray();
            }

            foreach (var component in componentsToOpen)
            {
                await component.OpenAsync(cancellationToken);
            }
        }

        protected override async Task OnCloseAsync(CancellationToken cancellationToken)
        {
            IComponent[] componentsToClose;
            lock (this.Lock)
            {
                var reverseList = new List<IComponent>(this.components);
                reverseList.Reverse();

                // Copy the collection into an array because component will be removed in OnInnerComponentClosed
                componentsToClose = reverseList.ToArray();
            }

            foreach (var component in componentsToClose)
            {
                await component.CloseAsync(cancellationToken);
            }
        }

        private void OnInnerComponentClosed(object sender, EventArgs args)
        {
            var component = (IComponent)sender;
            component.Closed -= this.onInnerComponentClosed;
            component.Faulted -= this.onInnerComponentFaulted;

            lock (this.Lock)
            {
                this.components.Remove(component);
            }
        }

        private void OnInnerComponentFaulted(object sender, FirstChanceExceptionEventArgs args)
        {
            var component = (IComponent)sender;
            component.Faulted -= this.onInnerComponentFaulted;
            this.Fault(args.Exception);
        }
    }
}
