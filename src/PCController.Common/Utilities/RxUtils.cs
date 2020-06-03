using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local
{
    public enum ChangeAction : byte
    {
        Added = 0,
        Removed = 1
    }

    public static class RxUtils
    {
        public static IDisposable SubscribeAsync<T>(this IObservable<T> observable, Func<T, CancellationToken, Task> func)
        {
            var cts = new CancellationTokenSource();
            return observable.Select(o =>
                {
                    cts?.Cancel();
                    cts = new CancellationTokenSource();
                    return Observable.FromAsync(() => func(o, cts.Token));
                })
                .Concat()
                .Subscribe();
        }

        public static IObservable<Change<T>> ToSetChanges<T>(this IObservable<IReadOnlyCollection<T>> observable)
        {
            return Observable.Create<Change<T>>(observer =>
            {
                var previousValue = new HashSet<T>();
                return observable.
                     Subscribe(newCollection =>
                     {
                         foreach (var removedItem in previousValue.Except(newCollection))
                         {
                             previousValue.Remove(removedItem);
                             observer.OnNext(new Change<T>(ChangeAction.Removed, removedItem));
                         }

                         foreach (var newItem in newCollection.Except(previousValue))
                         {
                             previousValue.Add(newItem);
                             observer.OnNext(new Change<T>(ChangeAction.Added, newItem));
                         }
                     });
            });
        }
    }

    public class Change<T>
    {
        public Change(ChangeAction action, T value)
        {
            Action = action;
            Value = value;
        }

        public ChangeAction Action { get; }

        public T Value { get; }
    }
}