using System;
using System.Collections.Generic;

namespace PCController.Local
{
    public class DisposableTracker : IDisposableTracker
    {
        private readonly List<IDisposable> disposables = new List<IDisposable>();
        private readonly object @lock = new object();
        private bool disposed;

        public void TrackDisposable(IDisposable disposable)
        {
            lock (this.@lock)
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(nameof(DisposableTracker));
                }

                this.disposables.Add(disposable);
            }
        }

        public void Dispose()
        {
            lock (this.@lock)
            {
                this.disposed = true;
            }

            foreach (var disposable in this.disposables)
            {
                disposable.Dispose();
            }

            this.disposables.Clear();
        }
    }
}
