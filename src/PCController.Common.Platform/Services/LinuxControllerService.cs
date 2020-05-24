using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PCController.Local.Services
{
    internal class LinuxControllerService : IControllerService
    {
        private readonly Config config;

        public LinuxControllerService(IOptions<Config> config)
        {
            this.config = config.Value;
        }

        public bool IsPlatformSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public async Task InvokeCommandAsync(string pin, Command command, CancellationToken cancellationToken)
        {
            if (this.config.Pin != pin)
            {
                throw new InvalidOperationException("Invalid PIN");
            }

            switch (command)
            {
                case Command.Shutdown:
                    await StartProcessAsync(@"sudo", "power off");
                    break;

                case Command.Sleep:
                    throw new NotImplementedException();

                case Command.Lock:
                    await StartProcessAsync(@"sudo", "reboot");
                    break;

                case Command.PlayPauseMedia:
                    throw new NotImplementedException();

                case Command.StopMedia:
                    throw new NotImplementedException();

                case Command.IncreaseVolume:
                    throw new NotImplementedException();

                case Command.DecreaseVolume:
                    throw new NotImplementedException();

                case Command.MuteVolume:
                    throw new NotImplementedException();

                case Command.LeftArrow:
                    throw new NotImplementedException();

                case Command.RightArrow:
                    throw new NotImplementedException();

                case Command.UpArrow:
                    throw new NotImplementedException();

                case Command.DownArrow:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException();
            }
        }

        private static async Task StartProcessAsync(string path, string args)
        {
            var process = Process.Start(path, args);
            await process.WaitForExitAsync();
        }
    }
}
