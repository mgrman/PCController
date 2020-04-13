using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface IRemoteControllerService
    {
        IReadOnlyList<RemoteServer> RemoteServers { get; }

        Task WakeUpAsync(RemoteServer remoteServer, CancellationToken cancellationToken);

        Task LockAsync(RemoteServer remoteServer, CancellationToken cancellationToken);

        Task SleepAsync(RemoteServer remoteServer, CancellationToken cancellationToken);

        Task ShutdownAsync(RemoteServer remoteServer, CancellationToken cancellationToken);
    }
}