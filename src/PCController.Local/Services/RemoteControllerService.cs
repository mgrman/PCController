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
                var pingSender = new Ping();
                OnlineStatus previousStatus = OnlineStatus.Unknown;
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // Create a buffer of 32 bytes of data to be transmitted.
                        int timeout = 120;
                        PingReply reply = await pingSender.SendPingAsync(remoteServer.Uri.Host, timeout);
                        if (reply.Status != IPStatus.Success)
                        {
                            previousStatus = OnlineStatus.Offline;
                            observer.OnNext(previousStatus);
                        }
                        else
                        {
                            if (previousStatus == OnlineStatus.Offline)
                            {
                                previousStatus = OnlineStatus.DeviceOnline;
                                observer.OnNext(previousStatus);
                            }
                            var routeUri = new Uri(ControllerController.StatusRoute, UriKind.Relative);
                            var res = new Uri(remoteServer.Uri, routeUri);
                            var message = new HttpRequestMessage(HttpMethod.Get, res);
                            var cancel = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                            var requestTask = _httpClient.SendAsync(message, cancel.Token);
                            var aaa = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromSeconds(10)));
                            if (aaa is Task<HttpResponseMessage> response && (await response).IsSuccessStatusCode)
                            {
                                previousStatus = OnlineStatus.ServerOnline;
                                observer.OnNext(previousStatus);
                            }
                            else
                            {
                                cancel.Cancel();
                                previousStatus = OnlineStatus.DeviceOnline;
                                observer.OnNext(previousStatus);
                            }
                        }
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                    catch
                    {
                        observer.OnNext(OnlineStatus.Unknown);
                        throw;
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
    }
}