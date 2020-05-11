using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCController.Local.Controller;

namespace PCController.Local.Services
{
    public class WindowsNativeExtensions : INativeExtensions
    {
        private readonly ILogger<WindowsNativeExtensions> _logger;
        private readonly IProcessHelper _processHelper;

        public WindowsNativeExtensions( ILogger<WindowsNativeExtensions> logger, IProcessHelper processHelper)
        {
            _logger = logger;
            _processHelper = processHelper;
        }

        public async Task<bool> PingServer(RemoteServer remoteServer, CancellationToken cancellationToken)
        {
            bool isPinged = await _processHelper.StartProcessAndCheckExitCodeAsync("ping", $"{remoteServer.Ip} -n 1", cancellationToken);


        return isPinged;
        }

        public bool IsPlatformSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public async Task<string> GetMac(RemoteServer remoteServer, CancellationToken cancellationToken)
        {
            await PingServer(remoteServer,cancellationToken);

            string arpResponse = await _processHelper.StartProcessAndReadOutAsync("arp", "-a", cancellationToken);
            

            var match = Regex.Match(arpResponse, $"^.*?({Regex.Escape(remoteServer.Ip)}).*?((?>[0-9A-Fa-f]{{2}}[:-]){{5}}(?>[0-9A-Fa-f]{{2}})).*?$", RegexOptions.Multiline);
            if (match.Success)
            {
                return match.Groups[2]
                    .Value;
            }
            else
            {
                return null;
            }
        }
    }
}
