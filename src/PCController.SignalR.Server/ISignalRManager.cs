using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    internal interface ISignalRManager
    {
        IObservable<IEnumerable<SignalRHubToClientConnectionServer>> Connections { get; }

        IObservable<SignalRHubToClientConnectionServer> NewConnectedDevices { get; }

        IObservable<SignalRHubToClientConnectionServer> NewDisconnectedDevices { get; }

        void AddClient(string connectionId, string machineName);

        void RemoveClient(string connectionId);

        Task InvokeCommandAsync(string connectionId, Command command, string pin, CancellationToken cancellationToken);
    }
}
