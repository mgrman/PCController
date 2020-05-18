using Microsoft.AspNetCore.SignalR;
using PCController.Local.Hubs;
using PCController.Local.Services;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local
{
    public class SignalRConnectionServer
    {
        private readonly IDisposableTracker _tracker=new DisposableTracker();
        private readonly BehaviorSubject<OnlineStatus> _isOnline;
        private readonly ISignalRManager _statusHub;

        public SignalRConnectionServer(ISignalRManager hubManager,string machineName, string signalRConnectionId)
        {
            _statusHub = hubManager;
            MachineName = machineName;
            SignalRConnectionId = signalRConnectionId;

            _isOnline = new BehaviorSubject<OnlineStatus>(OnlineStatus.Offline);

            _statusHub.NewDisconnectedDevices
                .Subscribe(o =>
                {
                    if (o.SignalRConnectionId == SignalRConnectionId)
                    {
                        _isOnline.OnNext(OnlineStatus.Offline);
                    }
                })
                .TrackSubscription(_tracker);
            _statusHub.NewConnectedDevices
                .Subscribe(o =>
                {
                    if (o.SignalRConnectionId == SignalRConnectionId)
                    {
                        _isOnline.OnNext(OnlineStatus.ServerOnline);
                    }
                })
                .TrackSubscription(_tracker);
        }

        public string MachineName { get; set; }
        public string SignalRConnectionId { get; }

        public string MacAddress { get; set; }

        public string Ip { get; }

        public string PIN { get; set; }
    

        public IObservable<OnlineStatus> IsOnline => _isOnline;

        public async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken)
        {
            _statusHub.InvokeCommandAsync(SignalRConnectionId, command, PIN, cancellationToken);
        }
    }
}