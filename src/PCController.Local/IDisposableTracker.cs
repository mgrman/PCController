using System;

namespace PCController.Local
{
    public interface IDisposableTracker : IDisposable
    {
        void TrackDisposable(IDisposable disposable);
    }
}