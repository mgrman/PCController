using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PCController.Local.Services;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http.Extensions;

namespace PCController.Local.Hubs
{
    public class StatusHub : Hub
    {
        public const string IDHeader = "PCController_ID";
        public const string RelativePath = "/statusHub";
        public const string InvokeCommandMethodName = nameof(InvokeCommand);
        public static readonly Uri RelativeUri = new Uri(RelativePath, UriKind.Relative);
        private readonly IControllerService _controllerService;
        private readonly IOptionsMonitor<Config> _config;
        private readonly ISignalRManager _signalRManager;

        public StatusHub(IControllerService controllerService, IOptionsMonitor<Config> config, ISignalRManager signalRManager)
        {
            _controllerService = controllerService;
            _config = config;
            _signalRManager = signalRManager;
        }

        public override Task OnConnectedAsync()
        {
            var httpCtx = Context.GetHttpContext();
            var id = httpCtx.Request.Headers[IDHeader].ToString();

            _signalRManager.AddClient(Context.ConnectionId, id);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _signalRManager.RemoveClient(Context.ConnectionId);

            return base.OnDisconnectedAsync(exception);
        }

        public async Task InvokeCommand(Command command, string pin)
        {
            if (!_controllerService.IsPlatformSupported)
            {
                throw new InvalidOperationException("PlatformNotSupported");
            }

            if (_config.CurrentValue.PIN != pin)
            {
                throw new InvalidOperationException("InvalidPIN");
            }

            await _controllerService.InvokeCommandAsync(command, CancellationToken.None);
        }
    }
}