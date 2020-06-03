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

            this.AdditionalInfo = new Dictionary<string, string>
            {
                { nameof(this.SignalRConnectionId), this.SignalRConnectionId }
            };
        }

        public string SignalRConnectionId { get; }

        public string MachineName { get; set; }

        public IReadOnlyDictionary<string, string> AdditionalInfo { get; }

        public IObservable<OnlineStatus> IsOnline => this.isOnline;

        public string InitialPin => string.Empty;

        public async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task InvokeCommandAsync(Command command, string pin, CancellationToken cancellationToken)
        {
            await this.statusHub.InvokeCommandAsync(this.SignalRConnectionId, command, pin, cancellationToken);
        }
    }
}