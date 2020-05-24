using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;

namespace PCController.Local.Shared
{
    public static class ObservableSectionExtensions
    {
        public static T Bind<T>(this IObservable<T> observable, ObservableSection section) => section.Bind(observable);

        public static Action<ChangeEventArgs> Set(this IObserver<string> observer, ObservableSection section) => section.Set(observer);

        public static Action<ChangeEventArgs> Set<T>(this IObserver<T> observer, Func<string, T> converter, ObservableSection section) => section.Set(observer, converter);
    }

    public class ObservableSection : ComponentBase, IDisposable
    {
        private readonly string _logPrefix;
        private readonly Dictionary<object, IDisposable> bindDisposableTracker = new Dictionary<object, IDisposable>();

        private readonly Dictionary<object, object> observableToValue = new Dictionary<object, object>();

        private readonly List<object> toDelete = new List<object>();

        private readonly HashSet<object> touchedObservables = new HashSet<object>();

        public ObservableSection()
        {
            this._logPrefix = $"[#{this.GetHashCode()}]";
        }

        /// <summary>
        ///     The content that will be displayed if the user is authorized.
        /// </summary>
        [Parameter]
        public RenderFragment<ObservableSection> ChildContent { get; set; }

        [Inject]
        protected ILogger<ObservableSection> logger { get; private set; }

        public virtual void Dispose()
        {
            foreach (var subscription in this.bindDisposableTracker.Values)
            {
                subscription.Dispose();
            }
        }

        public T Bind<T>(IObservable<T> observable)
        {
            this.logger.LogDebug($"{this._logPrefix}Bind {observable}[#{observable.GetHashCode()}] Exists:{this.observableToValue.ContainsKey(observable)}");
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
                    this.InvokeAsync(this.StateHasChanged);
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

        public Action<ChangeEventArgs> Set(IObserver<string> observer)
        {
            return e => observer.OnNext(e.Value.ToString());
        }

        public Action<ChangeEventArgs> Set<T>(IObserver<T> observer, Func<string, T> converter)
        {
            return e => observer.OnNext(converter(e.Value.ToString()));
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            this.ClearTouchFlag();

            builder.AddContent(0, this.ChildContent.Invoke(this));

            this.RemoveUnusedObservables();
        }

        private void ClearTouchFlag()
        {
            this.touchedObservables.Clear();
            this.logger.LogDebug($"{this._logPrefix}Starting binding ");
        }

        private void RemoveUnusedObservables()
        {
            foreach (var observable in this.observableToValue.Keys)
            {
                if (!this.touchedObservables.Contains(observable))
                {
                    this.toDelete.Add(observable);
                }
            }

            foreach (object o in this.toDelete)
            {
                if (this.bindDisposableTracker.Remove(o, out var subscription))
                {
                    subscription.Dispose();
                }

                this.observableToValue.Remove(o);
            }

            this.logger.LogDebug($"{this._logPrefix}RemoveUnusedObservables {this.toDelete.Count} Left:{this.observableToValue.Count}");
            this.toDelete.Clear();
        }
    }
}
