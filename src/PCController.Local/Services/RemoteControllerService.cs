using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCController.Local.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using PCController.Local.Hubs;

namespace PCController.Local.Services
{
    public class RemoteControllerService : IRemoteControllerService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RemoteControllerService> _logger;

        public RemoteControllerService(HttpClient httpClient, IOptionsSnapshot<Config> config, ILogger<RemoteControllerService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
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

            string arpResponse;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                await StartProcessAsync("/bin/ping", $"{ip} -c 1");
                arpResponse = await StartProcessAndReadOutAsync("/usr/sbin/arp", "-a");
            }
            else
            {
                await StartProcessAsync("ping", $"{ip} -n 1");
                arpResponse = await StartProcessAndReadOutAsync("arp", "-a");
            }

            var match = Regex.Match(arpResponse, $"^.*?({Regex.Escape(ip)}).*?((?>[0-9A-Fa-f]{{2}}[:-]){{5}}(?>[0-9A-Fa-f]{{2}})).*?$", RegexOptions.Multiline);
            var macAdress = match.Groups[2].Value;
            _logger.LogInformation($"MacAdress for {ip} was found as {macAdress}.");
            var mac = PhysicalAddress.Parse(macAdress);
            await IPAddress.Broadcast.SendWolAsync(mac);
        }

        public IObservable<OnlineStatus> IsOnline(RemoteServer remoteServer)
        {
            return Observable.Create<OnlineStatus>(async (observer, cancellationToken) =>
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        observer.OnCompleted();
                        return;
                    }
                    try
                    {
                        observer.OnNext(OnlineStatus.Offline);

                        var ip = remoteServer.Uri.Host;
                        if (ip == "localhost")
                        {
                            ip = "127.0.0.1";
                        }
                        bool isPinged;
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            isPinged = await StartProcessAndCheckExitCodeAsync("/bin/ping", $"{ip} -c 1");
                        }
                        else
                        {
                            isPinged = await StartProcessAndCheckExitCodeAsync("ping", $"{ip} -n 1");
                        }
                        if (!isPinged)
                        {
                            observer.OnNext(OnlineStatus.Offline);
                            await Task.Delay(1000);
                            continue;
                        }
                        observer.OnNext(OnlineStatus.DeviceOnline);

                        var hubConnection = new HubConnectionBuilder()
                            .WithUrl($"{remoteServer.Uri}statusHub")
                            .WithAutomaticReconnect()
                            .Build();

                        var tcs = new TaskCompletionSource<Exception>();
                        hubConnection.Closed += async (e) =>
                        {
                            tcs.SetResult(e);
                        };
                        hubConnection.Reconnected += async (e) =>
                        {
                        };

                        hubConnection.Reconnecting += async (e) =>
                        {
                        };

                        await hubConnection.StartAsync(cancellationToken);
                        observer.OnNext(OnlineStatus.ServerOnline);
                        await tcs.Task;
                    }
                    catch (Exception ex)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                }
            })
            .Distinct();
        }

        public async Task InvokeCommandAsync(Command command, RemoteServer remoteServer, CancellationToken cancellationToken)
        {
            await ExecuteRoute(remoteServer, command, cancellationToken);
        }

        private async Task ExecuteRoute(RemoteServer remoteServer, Command command, CancellationToken cancellationToken)
        {
            var content = new StringContent(string.Empty);
            content.Headers.Add(ControllerController.PinHeader, remoteServer.PIN);

            var routeUri = new Uri(ControllerController.CommandRoute.Replace(ControllerController.CommandPlaceholder, command.ToString()), UriKind.Relative);
            var res = new Uri(remoteServer.Uri, routeUri);

            var response = await _httpClient.PostAsync(res, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(response.StatusCode.ToString());
            }
        }

        private async Task StartProcessAsync(string path, string args)
        {
            var process = System.Diagnostics.Process.Start(path, args);
            await process.WaitForExitAsync();
        }

        private async Task<string> StartProcessAndReadOutAsync(string path, string args)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var process = System.Diagnostics.Process.Start(startInfo);
            await process.WaitForExitAsync();
            return await process.StandardOutput.ReadToEndAsync();
        }

        private async Task<bool> StartProcessAndCheckExitCodeAsync(string path, string args)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var process = System.Diagnostics.Process.Start(startInfo);
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
    }
}