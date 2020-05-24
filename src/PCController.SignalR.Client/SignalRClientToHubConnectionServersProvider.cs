using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using PCController.Common.DataTypes;

namespace PCController.Local.Services
{
    internal class SignalRClientToHubConnectionServersProvider : IRemoteServersProvider
    {
        public SignalRClientToHubConnectionServersProvider(IOptions<Config> config, INativeExtensions nativeExtensions, IControllerService controllerService)
        {
            var list = config.Value.RemoteServers.Select(o => new SignalRClientToHubConnectionServer(o, config.Value.Name, controllerService, nativeExtensions))
                .ToArray();

            this.RemoteServers = Observable.Return(list);
        }

        public IObservable<IReadOnlyList<IRemoteServer>> RemoteServers { get; }
    }
}
