using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local
{
    public class PCControllerComponentBase : ComponentBase, IDisposable, IDisposableTracker
    {
        private readonly DisposableTracker _disposableTracker = new DisposableTracker();

        public void TrackDisposable(IDisposable disposable)
        {
            _disposableTracker.TrackDisposable(disposable);
        }

        public virtual void Dispose()
        {
            _disposableTracker.Dispose();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            base.OnAfterRender(firstRender);
            if (!firstRender)
            {
                return;
            }
            await OnAfterFirstRenderAsync();
        }

        protected virtual Task OnAfterFirstRenderAsync()
        {
            return Task.CompletedTask;
        }
    }
}