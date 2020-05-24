using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace PCController.Local
{
    public class PcControllerComponentBase : ComponentBase, IDisposableTracker
    {
        private readonly DisposableTracker lifetimeDisposableTracker = new DisposableTracker();

        public virtual void Dispose()
        {
            this.lifetimeDisposableTracker.Dispose();
        }

        public void TrackDisposable(IDisposable disposable)
        {
            this.lifetimeDisposableTracker.TrackDisposable(disposable);
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
