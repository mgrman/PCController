using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using PCController.Common.DataTypes;
using PCController.Local.Hubs;
using PCController.SignalR.Common;

namespace PCController.Local.Services
{
    internal class SignalRHubToClientConnectionServersProvider : ISignalRManager, IRemoteServersProvider
    {
        private readonly ConcurrentDictionary<string, SignalRHubToClientConnectionServer> connectedIds = new ConcurrentDictionary<string, SignalRHubToClientConnectionServer>();

        private readonly BehaviorSubject<IReadOnlyList<SignalRHubToClientConnectionServer>> connections = new BehaviorSubject<IReadOnlyList<SignalRHubToClientConnectionServer>>(Array.Empty<SignalRHubToClientConnectionServer>());
        private readonly IHubContext<StatusHub> hub;
        private readonly Subject<SignalRHubToClientConnectionServer> newConnectedDevices = new Subject<SignalRHubToClientConnectionServer>();
        private readonly Subject<SignalRHubToClientConnectionServer> newDisconnectedDevices = new Subject<SignalRHubToClientConnectionServer>();

        public SignalRHubToClientConnectionServersProvider(IHubContext<StatusHub> hub)
        {
            this.hub = hub;
        }

        IObservable<IReadOnlyList<IRemoteServer>> IRemoteServersProvider.RemoteServers => this.connections;

        public IObservable<IEnumerable<SignalRHubToClientConnectionServer>> Connections => this.connections;

        public IObservable<SignalRHubToClientConnectionServer> NewConnectedDevices => this.newConnectedDevices;

        public IObservable<SignalRHubToClientConnectionServer> NewDisconnectedDevices => this.newDisconnectedDevices;

        public async Task InvokeCommandAsync(string connectionId, Command command, string pin, CancellationToken cancellationToken)
        {
            await this.hub.Clients.Client(connectionId)
                .SendAsync(SignalRConfig.InvokeCommandMethodName, command, pin, cancellationToken);
        }

        public void AddClient(string connectionId, string machineName)
        {
            var value = new SignalRHubToClientConnectionServer(this, machineName, connectionId);
            this.connectedIds[value.SignalRConnectionId] = value;
            this.newConnectedDevices.OnNext(value);
            this.connections.OnNext(this.connectedIds.Values.ToList());
        }

        public void RemoveClient(string connectionId)
        {
            if (this.connectedIds.Remove(connectionId, out var value))
            {
                this.newDisconnectedDevices.OnNext(value);
            }

            this.connections.OnNext(this.connectedIds.Values.ToList());
        }
    }
}
