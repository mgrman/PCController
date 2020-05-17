using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using PCController.Local.Hubs;
using PCController.Local.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local
{
    public class AutoRetryHub
    {
        private readonly HubConnection _hubConnection;
        private readonly ISubject<bool> _isOnline = new Subject<bool>();

        public AutoRetryHub(Uri serverUri, string machineID)
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(new Uri(serverUri, StatusHub.RelativeUri), o =>
                {
                    o.Headers.Add(StatusHub.IDHeader, machineID);
                })
                .WithAutomaticReconnect()
                .Build();
            _hubConnection.KeepAliveInterval = TimeSpan.FromMilliseconds(1000);
            _hubConnection.ServerTimeout = TimeSpan.FromMilliseconds(30000);
            _hubConnection.HandshakeTimeout = TimeSpan.FromMilliseconds(500);

            _hubConnection.Closed += async (e) =>
            {
                _isOnline.OnNext(false);
            };
            _hubConnection.Reconnecting += async (e) =>
            {
                _isOnline.OnNext(false);
            };
            _hubConnection.Reconnected += async (e) =>
            {
                _isOnline.OnNext(true);
            };

            IsActive.DistinctUntilChanged()
                .SelectMany(IsActiveChanged)
                .Subscribe();

            _isOnline.Subscribe(o =>
            {
                Console.WriteLine($"AutoRetryHub.IsOnline:{o}");
            });
        }

        public IObservable<bool> IsServerOnline => _isOnline;

        public ISubject<bool> IsActive { get; } = new BehaviorSubject<bool>(false);

        public async Task InvokeCommandAsync(Command command, string pin, CancellationToken cancellationToken)
        {
            await _hubConnection.SendAsync(StatusHub.InvokeCommandMethodName, command, pin, cancellationToken);
        }

        private async Task<Unit> IsActiveChanged(bool o, CancellationToken cancellationToken)
        {
            if (o)
            {
                await StartAsync(cancellationToken);
            }
            else
            {
                await StopAsync(cancellationToken);
            }

            return Unit.Default;
        }

        private async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_hubConnection.State == HubConnectionState.Disconnected)
                {
                    try
                    {
                        await _hubConnection.StartAsync(cancellationToken);
                    }
                    catch (HttpRequestException)
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }
                    _isOnline.OnNext(true);
                }

                return;
            }
        }

        private async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_hubConnection.State != HubConnectionState.Disconnected)
            {
                await _hubConnection.StopAsync(cancellationToken);
                _isOnline.OnNext(false);
            }
        }
    }
}