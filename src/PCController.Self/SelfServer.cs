using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PCController.Common.DataTypes;
using PCController.Local.Services;

namespace PCController.Local
{
    internal class SelfServer : IRemoteServer
    {
        private readonly IControllerService controllerService;
        private readonly string pin;

        public SelfServer(IOptions<Config> config, IControllerService controllerService)
        {
            this.controllerService = controllerService;
            this.pin = config.Value.Pin;
            this.IsOnline = Observable.Return(OnlineStatus.ServerOnline);
        }

        public string MachineName => "LOCAL";

        public IEnumerable<(string key, string value)> AdditionalInfo { get; } = Array.Empty<(string key, string value)>();


        public IObservable<OnlineStatus> IsOnline { get; }

        public Task InvokeCommandAsync(Command command, CancellationToken cancellationToken) => this.controllerService.InvokeCommandAsync(this.pin, command, cancellationToken);
    }
}
