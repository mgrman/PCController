using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PCController.Local.Controller;

namespace PCController.Local.Services
{
    public class ProcessHelper : IProcessHelper
    {
        private readonly ILogger<ProcessHelper> _logger;

        public ProcessHelper( ILogger<ProcessHelper> logger)
        {
            _logger = logger;
        }

        public async Task<bool> StartProcessAndCheckExitCodeAsync(string path, string args, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var process = System.Diagnostics.Process.Start(startInfo);
            await process.WaitForExitAsync(cancellationToken);
            return process.ExitCode == 0;
        }

        public async Task<string> StartProcessAndReadOutAsync(string path, string args, CancellationToken cancellationToken)
        {
            var startInfo = new ProcessStartInfo()
            {
                FileName = path,
                Arguments = args,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var process = System.Diagnostics.Process.Start(startInfo);
            await process.WaitForExitAsync(cancellationToken);
            return await process.StandardOutput.ReadToEndAsync();
        }
    }
}
