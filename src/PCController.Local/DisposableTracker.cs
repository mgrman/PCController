using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace PCController.Local
{
    public class DisposableTracker : IDisposableTracker
    {
        private object _lock = new object();
        private bool _disposed = false;
        private List<IDisposable> disposables = new List<IDisposable>();

        public void TrackDisposable(IDisposable disposable)
        {
            lock (_lock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(nameof(DisposableTracker));
                }
                disposables.Add(disposable);
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _disposed = true;
            }
            foreach (var disposable in disposables)
            {
                disposable.Dispose();
            }
            disposables.Clear();
        }
    }
}