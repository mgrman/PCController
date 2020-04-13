using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Local.Services
{
    public class ControllerService : IControllerService
    {
        public void InvokeCommand(Command command)
        {
            switch (command)
            {
                case Command.Shutdown:
                    Shutdown();
                    break;

                case Command.Sleep:
                    Sleep();
                    break;

                case Command.Lock:
                    Lock();
                    break;

                case Command.PlayMedia:
                case Command.PauseMedia:
                case Command.StopMedia:
                case Command.IncreaseVolume:
                case Command.DecreaseVolume:
                case Command.MuteVolume:
                default:
                    throw new NotImplementedException();
                    break;
            }
        }

        public void Sleep()
        {
            System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
        }

        public void Shutdown()
        {
            System.Diagnostics.Process.Start(@"shutdown", "/s");
        }

        private void Lock()
        {
            System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
        }
    }
}