using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using PCController.Common.DataTypes;
using PCController.Http.Server.Controllers;
using PCController.Local.Services;

namespace PCController.Local
{
    internal class HttpServer : INetworkAccessibleServer, IPinProtectedServer
    {
        private readonly HttpClient httpClient;
        private readonly BehaviourSubjectWithTracking<OnlineStatus> isOnline = new BehaviourSubjectWithTracking<OnlineStatus>(OnlineStatus.Unknown);
        private readonly INativeExtensions nativeExtensions;

        public HttpServer(RemoteServerConfig serverConfig, HttpClient httpClient, INativeExtensions nativeExtensions)
        {
            this.httpClient = httpClient;
            this.MachineName = serverConfig.Name;
            this.Uri = serverConfig.Uri;
            this.MacAddress = serverConfig.MacAddress;
            this.nativeExtensions = nativeExtensions;

            this.InitialPin = serverConfig.Pin;
            this.Ip = IPAddress.TryParse(this.Uri.Host, out var temp) ? temp : null;
            this.AdditionalInfo = new Dictionary<string, string>
            {
                { nameof(this.Uri), this.Uri.ToString() }
            };

            this.isOnline.OnSubscibersChanged.SubscribeAsync(async (enabled, cancellationToken) =>
            {
                if (!enabled)
                {
                    return;
                }

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Create a buffer of 32 bytes of data to be transmitted.
                        var timeout = 120;
                        var isOnline = await this.nativeExtensions.PingServerAsync(this.Ip, cancellationToken);
                        if (!isOnline)
                        {
                            if (this.isOnline.Value != OnlineStatus.Offline)
                            {
                                this.isOnline.OnNext(OnlineStatus.Offline);
                            }
                        }
                        else
                        {
                            var routeUri = new Uri(CommandsController.StatusRoute, UriKind.Relative);
                            var res = new Uri(this.Uri, routeUri);
                            var message = new HttpRequestMessage(HttpMethod.Get, res);
                            var cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                            var requestTask = this.httpClient.SendAsync(message, cancel.Token);
                            var aaa = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromSeconds(5)));
                            if (aaa is Task<HttpResponseMessage> response && (await response).IsSuccessStatusCode)
                            {
                                if (this.isOnline.Value != OnlineStatus.ServerOnline)
                                {
                                    this.isOnline.OnNext(OnlineStatus.ServerOnline);
                                }
                            }
                            else
                            {
                                cancel.Cancel();
                                if (this.isOnline.Value != OnlineStatus.DeviceOnline)
                                {
                                    this.isOnline.OnNext(OnlineStatus.DeviceOnline);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        if (this.isOnline.Value != OnlineStatus.Unknown)
                        {
                            this.isOnline.OnNext(OnlineStatus.Unknown);
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });
        }

        public Uri Uri { get; }

        public string MacAddress { get; }

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
            var content = new StringContent(string.Empty);
            content.Headers.Add(CommandsController.PinHeader, pin);

            var routeUri = new Uri(CommandsController.CommandRoute.Replace(CommandsController.CommandPlaceholder, command.ToString()), UriKind.Relative);
            var res = new Uri(this.Uri, routeUri);

            var response = await this.httpClient.PostAsync(res, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(response.StatusCode.ToString());
            }
        }
    }
}