using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PCController.Services
{
    public class ControllerService : IControllerService
    {
        public void Lock()
        {
            System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
        }

        public void Sleep()
        {
            System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
        }

        public void Shutdown()
        {
            System.Diagnostics.Process.Start(@"shutdown", "/s");
        }
    }
}