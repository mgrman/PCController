using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PCController.Local.Services
{
    public class WindowsControllerService : IControllerService
    {
        private readonly Config _config;
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;

        public bool IsPlatformSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public WindowsControllerService(IOptions<Config> config)
        {
            _config = config.Value;
        }
        
        public async Task InvokeCommandAsync(string pin, Command command, CancellationToken cancellationToken)
        {
            if (_config.PIN != pin)
            {
                return;
            }
            switch (command)
            {
                case Command.Shutdown:
                    await StartProcessAsync(@"shutdown", "/s");
                    break;

                case Command.Sleep:
                    await StartProcessAsync(@"C:\WINDOWS\system32\rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
                    break;

                case Command.Lock:
                    await StartProcessAsync(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
                    break;

                case Command.PlayPauseMedia:
                    keybd_event((byte) 0xB3, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                case Command.StopMedia:
                    keybd_event((byte) 0xB2, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                case Command.IncreaseVolume:
                    keybd_event((byte) 0xAF, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                case Command.DecreaseVolume:
                    keybd_event((byte) 0xAE, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                case Command.MuteVolume:
                    keybd_event((byte) 0xAD, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                case Command.LeftArrow:
                    keybd_event((byte) 0x25, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                case Command.RightArrow:
                    keybd_event((byte) 0x26, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                case Command.UpArrow:
                    keybd_event((byte) 0x27, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                case Command.DownArrow:
                    keybd_event((byte) 0x29, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private async Task StartProcessAsync(string path, string args)
        {
            var process = System.Diagnostics.Process.Start(path, args);
            await process.WaitForExitAsync();
        }
    }
}
