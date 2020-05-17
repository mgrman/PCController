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
using Microsoft.Extensions.Options;

namespace PCController.Local
{
    public class RemoteServer
    {
        private readonly INativeExtensions _nativeExtensions;
        private readonly AutoRetryHub _hub;
        private readonly BehaviourSubjectWithTracking<OnlineStatus> _isOnline;

        public RemoteServer(RemoteServerConfig serverConfig, INativeExtensions nativeExtensions, string machineID)
        {
            _nativeExtensions = nativeExtensions;
            Id = serverConfig.ID;
            Uri = serverConfig.Uri;
            MacAddress = serverConfig.MacAddress;

            PIN = serverConfig.PIN;
            Ip = Uri.Host;
            if (Ip == "localhost")
            {
                Ip = "127.0.0.1";
            }

            _hub = new AutoRetryHub(Uri, machineID);

            _isOnline = new BehaviourSubjectWithTracking<OnlineStatus>(OnlineStatus.Offline);
            _isOnline.OnSubscibersChanged.Subscribe(_hub.IsActive);

            _hub.IsServerOnline.SelectMany(IsOnlineChanged)
                .Subscribe();
        }

        public string Id { get; set; }

        public Uri Uri { get; set; }

        public string MacAddress { get; set; }

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