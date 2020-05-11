using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PCController.Local.Services;

namespace PCController.Local.Hubs
{
    public class StatusHub : Hub
    {
        private readonly IControllerService _controllerService;
        private readonly IOptionsMonitor<Config> _config;
        public const string RelativePath = "/statusHub";
        public static readonly Uri RelativeUri = new Uri(RelativePath, UriKind.Relative);

        public const string InvokeCommandMethodName = nameof(InvokeCommand);

        public StatusHub(IControllerService controllerService, IOptionsMonitor<Config> config)
        {
            _controllerService = controllerService;
            _config = config;
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