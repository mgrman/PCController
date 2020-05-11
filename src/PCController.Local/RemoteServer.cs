using Microsoft.AspNetCore.SignalR.Client;
using PCController.Local.Hubs;
using PCController.Local.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PCController.Local.Controller;

namespace PCController.Local
{
    public class RemoteServer
    {
        private readonly INativeExtensions _nativeExtensions;
        private readonly AutoRetryHub _hub;
        private readonly BehaviourSubjectWithTracking<OnlineStatus> _isOnline;
        private HttpClient _httpClient;

        public RemoteServer(RemoteServerConfig config, INativeExtensions nativeExtensions, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _nativeExtensions = nativeExtensions;
            Uri = config.Uri;
            PIN = config.PIN;
            Ip = Uri.Host;
            if (Ip == "localhost")
            {
                Ip = "127.0.0.1";
            }

            _hub = new AutoRetryHub(Uri);

            _isOnline = new BehaviourSubjectWithTracking<OnlineStatus>(OnlineStatus.Offline);
            _isOnline.OnSubscibersChanged.Subscribe(_hub.IsActive);

            _hub.IsServerOnline.SelectMany(IsOnlineChanged)
                .Subscribe();
        }

        public Uri Uri { get; set; }

        public string Ip { get; }

        public string PIN { get; set; }

        public IObservable<OnlineStatus> IsOnline => _isOnline;

        public async Task<bool> TryToWakeUpAsync(CancellationToken cancellationToken)
        {
            var macAdress = await _nativeExtensions.GetMac(this, cancellationToken);
            if (macAdress == null)
            {
                return false;
            }

            try
            {
                var mac = PhysicalAddress.Parse(macAdress);
                await IPAddress.Broadcast.SendWolAsync(mac);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken)
        {
            _hub.InvokeCommandAsync(command, PIN, cancellationToken);
        }

        private async Task<Unit> IsOnlineChanged(bool o, CancellationToken cancellationToken)
        {
            if (o)
            {
                _isOnline.OnNext(OnlineStatus.ServerOnline);
            }
            else
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    bool isPinged = await _nativeExtensions.PingServer(this, cancellationToken);
                    if (isPinged)
                    {
                        _isOnline.OnNext(OnlineStatus.DeviceOnline);
                    }
                    else
                    {
                        _isOnline.OnNext(OnlineStatus.Offline);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
                }
            }

            return Unit.Default;
        }
    }
}
