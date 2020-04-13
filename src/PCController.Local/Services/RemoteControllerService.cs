using Microsoft.Extensions.Options;
using PCController.Local.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public class RemoteControllerService : IRemoteControllerService
    {
        private readonly HttpClient _httpClient;

        public RemoteControllerService(HttpClient httpClient, IOptionsSnapshot<Config> config)
        {
            _httpClient = httpClient;
            RemoteServers = config.Value.RemoteServers;
        }

        public IReadOnlyList<RemoteServer> RemoteServers { get; }

        public async Task WakeUpAsync(RemoteServer remoteServer, CancellationToken cancellationToken)
        {
            var ip = remoteServer.Uri.Host;
            if (ip == "localhost")
            {
                ip = "127.0.0.1";
            }
            var res = await ArpRequest.SendAsync(IPAddress.Parse(ip));
            if (res.Exception != null)
            {
                throw res.Exception;
            }
            var mac = res.Address;
            await IPAddress.Broadcast.SendWolAsync(mac);
        }

        public async Task LockAsync(RemoteServer remoteServer, CancellationToken cancellationToken)
        {
            await ExecuteRoute(remoteServer, ControllerController.LockRoute, cancellationToken);
        }

        public async Task SleepAsync(RemoteServer remoteServer, CancellationToken cancellationToken)
        {
            await ExecuteRoute(remoteServer, ControllerController.SleepRoute, cancellationToken);
        }

        public async Task ShutdownAsync(RemoteServer remoteServer, CancellationToken cancellationToken)
        {
            await ExecuteRoute(remoteServer, ControllerController.ShutdownRoute, cancellationToken);
        }

        private async Task ExecuteRoute(RemoteServer remoteServer, string route, CancellationToken cancellationToken)
        {
            var content = new StringContent(string.Empty);
            content.Headers.Add(ControllerController.PinHeader, remoteServer.PIN);

            var routeUri = new Uri(route, UriKind.Relative);
            var res = new Uri(remoteServer.Uri, routeUri);

            var response = await _httpClient.PostAsync(res, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(response.StatusCode.ToString());
            }
        }
    }
}