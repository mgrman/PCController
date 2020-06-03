using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using PCController.Common.DataTypes;
using PCController.Local.Services;

namespace PCController.Local
{
    internal class SignalRClientToHubConnectionServer : INetworkAccessibleServer, IPinProtectedServer
    {
        private readonly AutoRetryHub hub;
        private readonly BehaviourSubjectWithTracking<OnlineStatus> isOnline;
        private readonly INativeExtensions nativeExtensions;

        public SignalRClientToHubConnectionServer(RemoteServerConfig serverConfig, string localMachineId, IControllerService controllerService, INativeExtensions nativeExtensions)
        {
            this.nativeExtensions = nativeExtensions;
            this.MachineName = serverConfig.Name;
            this.Uri = serverConfig.Uri;
            this.MacAddress = PhysicalAddress.TryParse(serverConfig.MacAddress, out var parsedMac) ? parsedMac : null;

            this.InitialPin = serverConfig.Pin;
            this.Ip = IPAddress.TryParse(this.Uri.Host, out var parsedIp) ? parsedIp : null;

            this.hub = new AutoRetryHub(this.Uri, localMachineId, controllerService);

            this.isOnline = new BehaviourSubjectWithTracking<OnlineStatus>(OnlineStatus.Offline);
            this.isOnline.OnSubscibersChanged.Subscribe(this.hub.IsActive);

            var cts = new CancellationTokenSource();
            this.hub.IsServerOnline.SubscribeAsync(this.IsOnlineChanged);

            this.AdditionalInfo = new Dictionary<string, string>
            {
                { nameof(this.Uri), this.Uri.ToString() },
                {nameof(this.MacAddress), this.MacAddress.ToString() }
            };
        }

        public Uri Uri { get; }

        public PhysicalAddress MacAddress { get; }

        public IPAddress Ip { get; }

        public string MachineName { get; }

        public IReadOnlyDictionary<string, string> AdditionalInfo { get; }

        public IObservable<OnlineStatus> IsOnline => this.isOnline;

        public string InitialPin { get; }

        public async Task WakeUpAsync(CancellationToken cancellationToken)
        {
            if (!this.nativeExtensions.IsPlatformSupported)
            {
                return;
            }

            await this.nativeExtensions.SendWolAsync(this.Ip, cancellationToken);
        }

        public async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async Task InvokeCommandAsync(Command command, string pin, CancellationToken cancellationToken)
        {
            await this.hub.InvokeCommandAsync(command, pin, cancellationToken);
        }

        private async Task<Unit> IsOnlineChanged(bool o, CancellationToken cancellationToken)
        {
            try
            {
                if (o)
                {
                    this.isOnline.OnNext(OnlineStatus.ServerOnline);
                }
                else
                {
                    if (!this.nativeExtensions.IsPlatformSupported)
                    {
                        this.isOnline.OnNext(OnlineStatus.Offline);
                    }
                    else
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var isPinged = await this.nativeExtensions.PingServerAsync(this.Ip, cancellationToken);
                            cancellationToken.ThrowIfCancellationRequested();
                            if (isPinged)
                            {
                                this.isOnline.OnNext(OnlineStatus.DeviceOnline);
                            }
                            else
                            {
                                this.isOnline.OnNext(OnlineStatus.Offline);
                            }

                            await Task.Delay(TimeSpan.FromMilliseconds(1000), cancellationToken);
                        }
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (OperationCanceledException)
            {
            }

            return Unit.Default;
        }
    }
}