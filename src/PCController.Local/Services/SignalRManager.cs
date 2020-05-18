using PCController.Local.Hubs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace PCController.Local.Services
{
    public class SignalRManager : ISignalRManager
    {
        private readonly IHubContext<StatusHub> hub;
        private ConcurrentDictionary<string, SignalRConnectionServer> _connectedIds = new ConcurrentDictionary<string, SignalRConnectionServer>();

        private Subject<IEnumerable<SignalRConnectionServer>> _connections = new Subject<IEnumerable<SignalRConnectionServer>>();
        private Subject<SignalRConnectionServer> _newConnectedDevices = new Subject<SignalRConnectionServer>();
        private Subject<SignalRConnectionServer> _newDisconnectedDevices = new Subject<SignalRConnectionServer>();

        public SignalRManager(IHubContext<StatusHub> hub)
        {
            this.hub = hub;
        }

        public IObservable<IEnumerable<SignalRConnectionServer>> Connections => _connections;

        public IObservable<SignalRConnectionServer> NewConnectedDevices => _newConnectedDevices;

        public IObservable<SignalRConnectionServer> NewDisconnectedDevices => _newDisconnectedDevices;

        public async Task InvokeCommandAsync(string connectionId, Command command, string pin, CancellationToken cancellationToken)
        {
            await hub.Clients.Client(connectionId).SendAsync("invoke", command, pin, cancellationToken);
        }

        public void AddClient(string connectionId, string machineName)
        {
            var value = new SignalRConnectionServer(this, machineName, connectionId);
            _connectedIds[value.SignalRConnectionId] = value;
            _newConnectedDevices.OnNext(value);
            _connections.OnNext(_connectedIds.Values);
        }

        public void RemoveClient(string connectionId)
        {
            if (_connectedIds.Remove(connectionId, out var value))
            {
                _newDisconnectedDevices.OnNext(value);
            }

            _connections.OnNext(_connectedIds.Values);
        }
    }
}