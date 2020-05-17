using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace PCController.Local.Services
{
    public class RemoteServersProvider : IRemoteServersProvider
    {
        public RemoteServersProvider(IOptions<Config> config, INativeExtensions nativeExtensions)
        {
            RemoteServers = config.Value.RemoteServers.Select(o => new RemoteServer(o, nativeExtensions, config.Value.ID))
                .ToArray();
        }

        public IReadOnlyList<RemoteServer> RemoteServers { get; }
    }
}