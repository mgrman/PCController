using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCController.Local.Services
{
    internal class WindowsNativeExtensions : INativeExtensions
    {
        private readonly ILogger<WindowsNativeExtensions> logger;
        private readonly IProcessHelper processHelper;

        public WindowsNativeExtensions(ILogger<WindowsNativeExtensions> logger, IProcessHelper processHelper)
        {
            this.logger = logger;
            this.processHelper = processHelper;
        }

        public async Task<bool> PingServerAsync(IPAddress remoteServer, CancellationToken cancellationToken)
        {
            var isPinged = await this.processHelper.StartProcessAndCheckExitCodeAsync("ping", $"{remoteServer} -n 1", cancellationToken);

            return isPinged;
        }

        public async Task<bool> SendWolAsync(IPAddress server, CancellationToken cancellationToken)
        {
            var macAdress = await this.GetMacAsync(server, cancellationToken);
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

        public bool IsPlatformSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public async Task<string> GetMacAsync(IPAddress remoteServer, CancellationToken cancellationToken)
        {
            await this.PingServerAsync(remoteServer, cancellationToken);

            var arpResponse = await this.processHelper.StartProcessAndReadOutAsync("arp", "-a", cancellationToken);

            var match = Regex.Match(arpResponse, $"^.*?({Regex.Escape(remoteServer.ToString())}).*?((?>[0-9A-Fa-f]{{2}}[:-]){{5}}(?>[0-9A-Fa-f]{{2}})).*?$", RegexOptions.Multiline);
            if (match.Success)
            {
                return match.Groups[2]
                    .Value;
            }

            return null;
        }
    }
}
