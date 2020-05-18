using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public interface ISignalRManager
    {
        void AddClient(string connectionId, string machineName);

        void RemoveClient(string connectionId);

        Task InvokeCommandAsync(string connectionId, Command command, string pin, CancellationToken cancellationToken);
        
        IObservable<IEnumerable<SignalRConnectionServer>> Connections { get; }

        IObservable<SignalRConnectionServer> NewConnectedDevices { get; }

        IObservable<SignalRConnectionServer> NewDisconnectedDevices { get; }
    }
}