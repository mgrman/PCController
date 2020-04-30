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

        IObservable<OnlineStatus> IsOnline(RemoteServer remoteServer);

        Task WakeUpAsync(RemoteServer remoteServer, CancellationToken cancellationToken);

        Task InvokeCommandAsync(Command command, RemoteServer remoteServer, CancellationToken cancellationToken);
    }
}