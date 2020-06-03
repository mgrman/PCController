using System;
using System.Collections;
using System.Collections.Generic;

namespace PCController.Local
{
    public interface IDisposableTracker : IDisposable
    {
        void TrackDisposable(IDisposable disposable);
    }

    public static class DisposableTrackerExtensions
    {
        public static void TrackSubscription(this IDisposable disposable, IDisposableTracker disposableTracker)
        {
            disposableTracker.TrackDisposable(disposable);
        }

        public static void TrackSubscription(this IDisposable disposable, IList<IDisposable> disposableTracker)
        {
            disposableTracker.Add(disposable);
        }

        public static void DisposeAll(this IEnumerable<IDisposable> disposableTracker)
        {
            foreach (var disposable in disposableTracker)
            {
                disposable.Dispose();
            }
        }
    }
}