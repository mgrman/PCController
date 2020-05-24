using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace PCController.Local.Services
{
    internal class WindowsControllerService : IControllerService
    {
        private const uint KeyeventfExtendedkey = 0x0001;
        private readonly Config config;

        public WindowsControllerService(IOptions<Config> config)
        {
            this.config = config.Value;
        }

        public bool IsPlatformSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public async Task InvokeCommandAsync(string pin, Command command, CancellationToken cancellationToken)
        {
            if (this.config.Pin != pin)
            {
                throw new InvalidOperationException("Invalid PIN");
            }

            switch (command)
            {
                case Command.Shutdown:
                    await this.StartProcessAsync(@"shutdown", "/s");
                    break;

                case Command.Sleep:
                    await this.StartProcessAsync(@"C:\WINDOWS\system32\rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
                    break;

                case Command.Lock:
                    await this.StartProcessAsync(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
                    break;

                case Command.PlayPauseMedia:
                    keybd_event(0xB3, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                case Command.StopMedia:
                    keybd_event(0xB2, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                case Command.IncreaseVolume:
                    keybd_event(0xAF, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                case Command.DecreaseVolume:
                    keybd_event(0xAE, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                case Command.MuteVolume:
                    keybd_event(0xAD, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                case Command.LeftArrow:
                    keybd_event(0x25, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                case Command.RightArrow:
                    keybd_event(0x26, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                case Command.UpArrow:
                    keybd_event(0x27, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                case Command.DownArrow:
                    keybd_event(0x29, 0, KeyeventfExtendedkey | 0, 0);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private async Task StartProcessAsync(string path, string args)
        {
            var process = Process.Start(path, args);
            await process.WaitForExitAsync();
        }
    }
}
