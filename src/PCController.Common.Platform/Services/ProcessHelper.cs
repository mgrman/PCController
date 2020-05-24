using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PCController.Local.Services
{
    internal class ProcessHelper : IProcessHelper
    {
        private readonly ILogger<ProcessHelper> logger;

        public ProcessHelper(ILogger<ProcessHelper> logger)
        {
            this.logger = logger;
        }

        public async Task<bool> StartProcessAndCheckExitCodeAsync(string path, string args, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var process = Process.Start(startInfo);
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }

        public async Task<string> StartProcessAndReadOutAsync(string path, string args, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var process = Process.Start(startInfo);
            await process.WaitForExitAsync(cancellationToken);
            return await process.StandardOutput.ReadToEndAsync();
        }
    }
}
