using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local
{
    public static class RxUtils
    {
        public static IDisposable SubscribeAsync<T>(this IObservable<T> observable, Func<T, CancellationToken, Task> func)
        {
            var cts = new CancellationTokenSource();
            return observable
                .Select(o =>
                {
                    cts?.Cancel();
                    cts = new CancellationTokenSource();
                    return Observable.FromAsync(() => func(o, cts.Token));
                })
                .Concat()
                .Subscribe();
        }

        public static void TrackSubscription(this IDisposable disposable, IDisposableTracker disposableTracker)
        {
            disposableTracker.TrackDisposable(disposable);
        }
    }
}