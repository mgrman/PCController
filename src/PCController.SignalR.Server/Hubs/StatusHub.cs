using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PCController.Local.Services;
using PCController.SignalR.Common;

namespace PCController.Local.Hubs
{
    internal class StatusHub : Hub
    {
        private readonly IControllerService controllerService;
        private readonly ISignalRManager signalRManager;

        public StatusHub(IControllerService controllerService, ISignalRManager signalRManager)
        {
            this.controllerService = controllerService;
            this.signalRManager = signalRManager;
        }

        public override Task OnConnectedAsync()
        {
            var httpCtx = this.Context.GetHttpContext();
            var id = httpCtx.Request.Headers[SignalRConfig.IdHeader]
                .ToString();

            this.signalRManager.AddClient(this.Context.ConnectionId, id);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            this.signalRManager.RemoveClient(this.Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        [HubMethodName(SignalRConfig.InvokeCommandMethodName)]
        public async Task InvokeCommand(Command command, string pin)
        {
            if (!this.controllerService.IsPlatformSupported)
            {
                throw new InvalidOperationException("PlatformNotSupported");
            }

            await this.controllerService.InvokeCommandAsync(pin, command, CancellationToken.None);
        }
    }
}
