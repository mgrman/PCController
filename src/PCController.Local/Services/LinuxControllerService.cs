using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public class LinuxControllerService : IControllerService
    {
        public bool IsPlatformSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        public async Task InvokeCommandAsync(Command command, CancellationToken cancellationToken)
        {
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

        private async Task StartProcessAsync(string path, string args)
        {
            var process = System.Diagnostics.Process.Start(path, args);
            await process.WaitForExitAsync();
        }
    }
}