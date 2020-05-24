using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using PCController.Local.Services;
using PCController.SignalR.Common;

namespace PCController.Local
{
    internal class AutoRetryHub
    {
        private readonly HubConnection hubConnection;
        private readonly ISubject<bool> isOnline = new Subject<bool>();

        public AutoRetryHub(Uri serverUri, string machineId, IControllerService controllerService)
        {
            this.hubConnection = new HubConnectionBuilder().WithUrl(new Uri(serverUri, SignalRConfig.RelativeUri),
                    o =>
                    {
                        o.Headers.Add(SignalRConfig.IdHeader, machineId);
                    })
                .WithAutomaticReconnect()
                .Build();
            this.hubConnection.KeepAliveInterval = TimeSpan.FromMilliseconds(1000);
            this.hubConnection.ServerTimeout = TimeSpan.FromMilliseconds(30000);
            this.hubConnection.HandshakeTimeout = TimeSpan.FromMilliseconds(500);

            this.hubConnection.Closed += async e =>
            {
                this.isOnline.OnNext(false);
            };
            this.hubConnection.Reconnecting += async e =>
            {
                this.isOnline.OnNext(false);
            };
            this.hubConnection.Reconnected += async e =>
            {
                this.isOnline.OnNext(true);
            };
            this.hubConnection.On<Command, string>(SignalRConfig.InvokeCommandMethodName,
                async (cmd, pin) =>
                {
                    await controllerService.InvokeCommandAsync(pin, cmd, CancellationToken.None);
                });

            this.IsActive.DistinctUntilChanged()
                .SelectMany(this.IsActiveChanged)
                .Subscribe();
        }

        public IObservable<bool> IsServerOnline => this.isOnline;

        public ISubject<bool> IsActive { get; } = new BehaviorSubject<bool>(false);

        public async Task InvokeCommandAsync(Command command, string pin, CancellationToken cancellationToken)
        {
            await this.hubConnection.SendAsync(SignalRConfig.InvokeCommandMethodName, command, pin, cancellationToken);
        }

        private async Task<Unit> IsActiveChanged(bool o, CancellationToken cancellationToken)
        {
            if (o)
            {
                await this.StartAsync(cancellationToken);
            }
            else
            {
                await this.StopAsync(cancellationToken);
            }

            return Unit.Default;
        }

        private async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (this.hubConnection.State == HubConnectionState.Disconnected)
                {
                    try
                    {
                        await this.hubConnection.StartAsync(cancellationToken);
                    }
                    catch (Exception)
                    {
                        await Task.Delay(1000, cancellationToken);
                        continue;
                    }

                    this.isOnline.OnNext(true);
                }

                return;
            }
        }

        private async Task StopAsync(CancellationToken cancellationToken)
        {
            if (this.hubConnection.State != HubConnectionState.Disconnected)
            {
                await this.hubConnection.StopAsync(cancellationToken);
                this.isOnline.OnNext(false);
            }
        }
    }
}
