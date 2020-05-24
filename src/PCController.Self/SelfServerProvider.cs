using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Microsoft.Extensions.Options;
using PCController.Common.DataTypes;

namespace PCController.Local.Services
{
    internal class SelfServerProvider : IRemoteServersProvider
    {
        public SelfServerProvider(IOptions<Config> config, IControllerService controllerService)
        {
            this.RemoteServers = Observable.Return(new[]
            {
                new SelfServer(config, controllerService)
            });
        }

        public IObservable<IReadOnlyList<IRemoteServer>> RemoteServers { get; }
    }
}
