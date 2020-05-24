using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace PCController.Local
{
    public class PcControllerComponentBase : ComponentBase2, IDisposableTracker
    {
        private readonly Dictionary<object, IDisposable> bindDisposableTracker = new Dictionary<object, IDisposable>();
        private readonly DisposableTracker lifetimeDisposableTracker = new DisposableTracker();

        private readonly Dictionary<object, object> observableToValue = new Dictionary<object, object>();
        private readonly HashSet<object> touchedObservables = new HashSet<object>();

        public virtual void Dispose()
        {
            this.lifetimeDisposableTracker.Dispose();

            foreach (var subscription in this.bindDisposableTracker.Values)
            {
                subscription.Dispose();
            }
        }

        public void TrackDisposable(IDisposable disposable)
        {
            this.lifetimeDisposableTracker.TrackDisposable(disposable);
        }

        protected T Bind<T>(IObservable<T> observable)
        {
            Console.WriteLine($"Bind {observable} {observable.GetHashCode()} Exists:{this.observableToValue.ContainsKey(observable)}");
            this.touchedObservables.Add(observable);
            if (this.observableToValue.ContainsKey(observable))
            {
                return (T) this.observableToValue[observable];
            }

            var suspendInBind = true;
            var subscription = observable.Subscribe(o =>
            {
                this.observableToValue[observable] = o;
                if (!suspendInBind)
                {
                    InvokeAsync(this.StateHasChanged);
                }
            });
            this.bindDisposableTracker[observable] = subscription;
            suspendInBind = false;
            if (this.observableToValue.ContainsKey(observable))
            {
                return (T) this.observableToValue[observable];
            }

            return default;
        }

        protected override void OnParametersSet()
        {
            Console.WriteLine("OnParametersSet");
            base.OnParametersSet();
        }

        protected override void OnBeforeRender()
        {
            Console.WriteLine("OnBeforeRender");
            base.OnBeforeRender();
            this.touchedObservables.Clear();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            Console.WriteLine($"OnAfterRender {firstRender} bound:{this.observableToValue.Keys.Count} touched:{touchedObservables.Count}");
            var toDelete = new List<object>();
            foreach (var observable in this.observableToValue.Keys)
            {
                if (!this.touchedObservables.Contains(observable))
                {
                    toDelete.Add(observable);
                }
            }

            foreach (object o in toDelete)
            {
                if (this.bindDisposableTracker.Remove(o, out var subscription))
                {
                    subscription.Dispose();
                }

                this.observableToValue.Remove(o);
            }

        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (!firstRender)
            {
                return;
            }

            await this.OnAfterFirstRenderAsync();
        }

        protected virtual Task OnAfterFirstRenderAsync() => Task.CompletedTask;
    }
}
