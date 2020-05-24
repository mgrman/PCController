using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using PCController.Common.DataTypes;
using PCController.Local.Services;

namespace PCController.Local
{
    internal class SignalRHubToClientConnectionServer : IRemoteServer, IPinProtectedServer
    {
        private readonly BehaviorSubject<OnlineStatus> isOnline;
        private readonly BehaviorSubject<string> pin = new BehaviorSubject<string>("");
        private readonly ISignalRManager statusHub;
        private readonly IDisposableTracker tracker = new DisposableTracker();

        public SignalRHubToClientConnectionServer(ISignalRManager hubManager, string machineName, string signalRConnectionId)
        {
            this.statusHub = hubManager;
            this.MachineName = machineName;
            this.SignalRConnectionId = signalRConnectionId;

            this.isOnline = new BehaviorSubject<OnlineStatus>(OnlineStatus.Offline);

            this.statusHub.NewDisconnectedDevices.Subscribe(o =>
                {
                    if (o.SignalRConnectionId == this.SignalRConnectionId)
                    {
                        this.isOnline.OnNext(OnlineStatus.Offline);
                    }
                })
                .TrackSubscription(this.tracker);
            this.statusHub.NewConnectedDevices.Subscribe(o =>
                {
                    if (o.SignalRConnectionId == this.SignalRConnectionId)
                    {
                        this.isOnline.OnNext(OnlineStatus.ServerOnline);
                    }
                })
                .TrackSubscription(this.tracker);

            this.AdditionalInfo = new[]
            {
                (nameof(this.SignalRConnectionId), this.SignalRConnectionId)
            };
        }

        public string SignalRConnectionId { get; }

        public ISubject<string> Pin => this.pin;

        public string MachineName { get; set; }

        public IEnumerable<(string key, string value)> AdditionalInfo { get; }

        public IObservable<OnlineStatus> IsOnline => this.isOnline;

        public async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken) => this.statusHub.InvokeCommandAsync(this.SignalRConnectionId, command, this.pin.Value, cancellationToken);
    }
}
