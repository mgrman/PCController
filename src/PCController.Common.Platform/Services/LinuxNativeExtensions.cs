using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCController.Local.Services
{
    internal class LinuxNativeExtensions : INativeExtensions
    {
        private readonly ILogger<LinuxNativeExtensions> logger;
        private readonly IProcessHelper processHelper;

        public LinuxNativeExtensions(ILogger<LinuxNativeExtensions> logger, IProcessHelper processHelper)
        {
            this.logger = logger;
            this.processHelper = processHelper;
        }

        public async Task<bool> PingServerAsync(IPAddress remoteServer, CancellationToken cancellationToken)
        {
            var isPinged = await this.processHelper.StartProcessAndCheckExitCodeAsync("/bin/ping", $"{remoteServer} -c 1", cancellationToken);
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

        public bool IsPlatformSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public async Task<string> GetMacAsync(IPAddress remoteServer, CancellationToken cancellationToken)
        {
            await this.PingServerAsync(remoteServer, cancellationToken);

            var arpResponse = await this.processHelper.StartProcessAndReadOutAsync("/usr/sbin/arp", "-a", cancellationToken);

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
