using PCController.Local.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public class RemoteControllerService : IRemoteControllerService
    {
        private readonly HttpClient _httpClient;

        public RemoteControllerService(HttpClient httpClient, Config config)
        {
            _httpClient = httpClient;
            RemoteServers = config.RemoteServers;
        }

        public IReadOnlyList<RemoteServer> RemoteServers { get; }

        public async Task WakeUpAsync(RemoteServer remoteServer, CancellationToken cancellationToken)
        {
            var process = Process.Start("wolcmd", $"{remoteServer.Uri.Host} 255.255.255.0 7");
            await process.WaitForExitAsync(cancellationToken);
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