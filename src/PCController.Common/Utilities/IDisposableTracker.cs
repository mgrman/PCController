using System;

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
    }
}
