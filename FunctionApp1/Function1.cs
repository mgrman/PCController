using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;

namespace FunctionApp1
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([QueueTrigger("commandsqueue")]string myQueueItem, TraceWriter log)
        {
            log.Info($"Evaluating: {myQueueItem}");
            if (myQueueItem == "lock")
            {
                System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "user32.dll,LockWorkStation");
            }
            else if (myQueueItem == "sleep")
            {
                System.Diagnostics.Process.Start(@"C:\WINDOWS\system32\rundll32.exe", "powrprof.dll,SetSuspendState 0,1,0");
            }
            else
            {
                log.Info($"Unknown command: {myQueueItem}");
            }
        }
    }
}